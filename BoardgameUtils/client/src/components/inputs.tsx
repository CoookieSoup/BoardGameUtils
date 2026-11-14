import { useState } from "react";

type TextInputProps = {
    type?: string;
    name?: string;
    onChange?: (value: string) => void;
    placeholder: string;
    validate?: (value: string) => boolean;
};

function TextInput({ type, name, onChange, placeholder, validate }: TextInputProps) {
    const [value, setValue] = useState("");
    const [submitted, setSubmitted] = useState(false);

    const isValid = validate ? validate(value) : true;

    return (
        <>
            <input
                type={type || "text"}
                name={name}
                onChange={(e) => {
                    setValue(e.target.value);
                    onChange?.(e.target.value);
                }}
                placeholder={placeholder}
                required
                onBlur={() => setSubmitted(true)}
            />
            {submitted && !isValid && (
                <p className="error">Invalid value</p>
            )}
        </>
    );
}

export default TextInput;