import { useState, type ReactNode } from "react";

type AsyncButtonProps = {
    onClick: () => Promise<unknown>;
    children: ReactNode;
};

function AsyncButton({ onClick, children }: AsyncButtonProps) {
    const [isBusy, setIsBusy] = useState(false);

    const handleClick = async () => {
        setIsBusy(true);
        try {
            await onClick();
        } finally {
            setIsBusy(false);
        }
    };

    return (
        <button disabled={isBusy} onClick={handleClick}>
            {children}
        </button>
    );
}

export default AsyncButton;
