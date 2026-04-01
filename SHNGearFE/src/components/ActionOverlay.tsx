import { CheckCircle2, Loader2 } from 'lucide-react';

type ActionOverlayPhase = 'loading' | 'success';

interface ActionOverlayProps {
  open: boolean;
  phase: ActionOverlayPhase;
  loadingText?: string;
  successText?: string;
}

export default function ActionOverlay({
  open,
  phase,
  loadingText = 'Processing...',
  successText = 'Completed successfully',
}: ActionOverlayProps) {
  if (!open) {
    return null;
  }

  const isLoading = phase === 'loading';

  return (
    <div className="fixed inset-0 z-[90] flex items-center justify-center bg-slate-900/35 backdrop-blur-sm">
      <div className="w-[min(90vw,360px)] rounded-2xl border border-white/60 bg-white/90 px-8 py-7 text-center shadow-2xl">
        <div className="mx-auto mb-4 flex h-16 w-16 items-center justify-center rounded-full bg-slate-100">
          {isLoading ? (
            <Loader2 className="h-8 w-8 animate-spin text-slate-700" />
          ) : (
            <CheckCircle2 className="h-10 w-10 text-emerald-500" />
          )}
        </div>
        <p className="text-base font-semibold text-slate-900">{isLoading ? loadingText : successText}</p>
      </div>
    </div>
  );
}
