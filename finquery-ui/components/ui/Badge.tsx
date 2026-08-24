import React from 'react';

interface BadgeProps {
  variant?: 'blue' | 'teal' | 'emerald' | 'amber' | 'rose' | 'neutral';
  size?: 'sm' | 'md';
  children: React.ReactNode;
  icon?: React.ReactNode;
  className?: string;
}

export const Badge: React.FC<BadgeProps> = ({
  variant = 'blue',
  size = 'md',
  children,
  icon,
  className = '',
}) => {
  const styles: Record<string, string> = {
    blue: 'bg-blue-500/10 text-blue-400 border-blue-500/30',
    teal: 'bg-teal-500/10 text-teal-400 border-teal-500/30',
    emerald: 'bg-emerald-500/10 text-emerald-400 border-emerald-500/30',
    amber: 'bg-amber-500/10 text-amber-400 border-amber-500/30',
    rose: 'bg-rose-500/10 text-rose-400 border-rose-500/30',
    neutral: 'bg-slate-700/30 text-slate-300 border-slate-600/40',
  };

  const sizes = {
    sm: 'text-[11px] px-1.5 py-0.5 font-medium gap-1',
    md: 'text-xs px-2.5 py-1 font-medium gap-1.5',
  };

  const styleMap: Record<string, React.CSSProperties> = {
    blue: { background: '#2f2f2f', color: 'var(--text-primary)' },
    teal: { background: '#2f2f2f', color: 'var(--text-primary)' },
    emerald: { background: '#2f2f2f', color: 'var(--text-primary)' },
    amber: { background: '#2f2f2f', color: 'var(--text-primary)' },
    rose: { background: '#2f2f2f', color: 'var(--text-primary)' },
    neutral: { background: '#2f2f2f', color: 'var(--text-primary)' },
  };

  return (
    <span
      style={{
        display: 'inline-flex',
        alignItems: 'center',
        borderRadius: '6px',
        border: 'none',
        ...styleMap[variant],
        fontSize: size === 'sm' ? '11px' : '12px',
        padding: size === 'sm' ? '2px 6px' : '3px 9px',
        gap: '4px',
        lineHeight: 1.2,
      }}
      className={className}
    >
      {icon && <span style={{ display: 'inline-flex', alignItems: 'center' }}>{icon}</span>}
      {children}
    </span>
  );
};
