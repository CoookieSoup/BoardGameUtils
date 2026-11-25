import { useState, type ReactNode } from "react";

type AsyncFormProps = {
    className?: string;
    submitLabel: string;
    onSubmit: (form: HTMLFormElement) => Promise<unknown>;
    children: ReactNode;
};

function AsyncForm({ className, submitLabel, onSubmit, children }: AsyncFormProps) {
    const [isBusy, setIsBusy] = useState(false);

    const handleSubmit = async (e: React.SubmitEvent<HTMLFormElement>) => {
        e.preventDefault();
        setIsBusy(true);
        try {
            await onSubmit(e.currentTarget);
        } finally {
            setIsBusy(false);
        }
    };

    return (
        <form className={className} onSubmit={handleSubmit}>
            {children}
            <button type="submit" disabled={isBusy}>{submitLabel}</button>
        </form>
    );
}

export default AsyncForm;
