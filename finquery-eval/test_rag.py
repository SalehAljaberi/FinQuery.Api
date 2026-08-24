import os
os.environ["DEEPEVAL_DISABLE_WRITE_CACHE"] = "YES"  # Bypass portalocker NoneType crash on Windows
import json
import pytest
import requests
from typing import List
from deepeval import assert_test
from deepeval.metrics import AnswerRelevancyMetric, FaithfulnessMetric, ContextualPrecisionMetric
from deepeval.test_case import LLMTestCase
from deepeval.models import DeepEvalBaseLLM

# ---------------------------------------------------------
# 1. Setup Local Ollama as the "Judge" Model for DeepEval
# ---------------------------------------------------------
class LocalOllamaJudge(DeepEvalBaseLLM):
    def __init__(self, model_name="llama3.1:8b"):
        self.model_name = model_name

    def load_model(self):
        return self.model_name

    def generate(self, prompt: str) -> str:
        # Calls local Ollama running on standard port 11434
        response = requests.post(
            'http://localhost:11434/api/generate',
            json={"model": self.model_name, "prompt": prompt, "stream": False},
            timeout=600  # 10 minutes — local Ollama judge is slow
        )
        return response.json()['response']

    async def a_generate(self, prompt: str) -> str:
        # DeepEval prefers async, but we can wrap the sync call for simplicity
        return self.generate(prompt)

    def get_model_name(self):
        return self.model_name

# Initialize our judge (ensure you have this model pulled via `ollama run llama3.1:8b`)
custom_ollama_judge = LocalOllamaJudge(model_name="llama3.1:8b")

# ---------------------------------------------------------
# 2. Helper to query the C# RAG API (Handles SSE streams)
# ---------------------------------------------------------
def query_rag_api(question: str) -> dict:
    """
    Sends a question to the C# backend and parses the Server-Sent Events (SSE)
    to extract the final answer and the retrieved chunks.
    """
    url = "http://localhost:5000/api/chat"
    payload = {
        "question": question,
        "mode": "pdf", # Assuming PDF mode for eval
        "conversationHistory": []
    }
    
    response = requests.post(url, json=payload, stream=True)
    
    actual_output = ""
    retrieval_context = []

    for line in response.iter_lines():
        if line:
            decoded_line = line.decode('utf-8').strip()
            if decoded_line.startswith("data:"):
                data_str = decoded_line[5:].strip()
                if data_str == "[DONE]":
                    break
                try:
                    data = json.loads(data_str)
                    if data.get("type") == "sources":
                        # Extract the text of the chunks retrieved by pgvector + BM25
                        retrieval_context = [src.get("ChunkText", "") for src in data.get("sources", [])]
                    elif data.get("type") == "token":
                        # Append the streaming tokens to form the final answer
                        actual_output += data.get("token", "")
                except json.JSONDecodeError:
                    pass

    return {
        "actual_output": actual_output.strip(),
        "retrieval_context": retrieval_context
    }

# ---------------------------------------------------------
# 3. Load Dataset and Define the DeepEval Test
# ---------------------------------------------------------
def load_dataset():
    with open("test_dataset.json", "r", encoding="utf-8") as f:
        return json.load(f)

# Pytest parametrize allows us to run this test function for every item in our JSON
@pytest.mark.parametrize("test_data", load_dataset())
def test_rag_pipeline(test_data):
    input_question = test_data["input"]
    expected_output = test_data["expected_output"]

    # 1. Get actual output and context from our C# backend
    rag_response = query_rag_api(input_question)
    actual_output = rag_response["actual_output"]
    retrieval_context = rag_response["retrieval_context"]

    # 2. Package it into a DeepEval test case
    test_case = LLMTestCase(
        input=input_question,
        actual_output=actual_output,
        expected_output=expected_output,
        retrieval_context=retrieval_context
    )

    # 3. Define the Metrics (Using our local Ollama Judge)
    answer_relevancy = AnswerRelevancyMetric(threshold=0.8, model=custom_ollama_judge)
    faithfulness = FaithfulnessMetric(threshold=0.8, model=custom_ollama_judge)
    contextual_precision = ContextualPrecisionMetric(threshold=0.8, model=custom_ollama_judge)

    # 4. Run the assertions
    assert_test(test_case, [answer_relevancy, faithfulness, contextual_precision])
