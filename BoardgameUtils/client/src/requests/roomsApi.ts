import toast from "react-hot-toast";
import type { NemesisPlayerObjective } from "../types/nemesisTypes";

const API = "http://localhost:5018";

type ApiResponse<T> = {
    success: boolean;
    data?: T;
    message?: string;
};

export type JoinCreateRoomResponse = {
    playerId: number;
    roomCode: string;
    roomId: number;
};

export type PlayerInfoResponse = {
    nickname: string;
    objective?: {
        title: string;
        type: number;
    };
};

export const apiRequest = async <T>(
    url: string,
    options?: RequestInit,
    successMessage?: string
): Promise<T> => {
    try {
        const res = await fetch(url, {
            ...options,
            headers: {
                "X-Tenant-Id": import.meta.env.VITE_TENANT_ID,
                ...(options?.headers as Record<string, string>),
            },
        });
        const response: ApiResponse<T> = await res.json();

        if (!res.ok || !response.success) {
            throw new Error(response.message || "Something went wrong");
        }

        if (successMessage) {
            toast.success(successMessage);
        }

        return response.data as T;
    } catch (error) {
        const message = error instanceof Error
            ? error.message
            : "Something went wrong";

        toast.error(message);
        throw error;
    }
};

export const joinRoom = (code: string, name: string) => {
    return apiRequest<JoinCreateRoomResponse>(
        `${API}/api/rooms/${code}/join`,
        {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ name }),
        },
        "Joined room successfully!"
    );
};

export const createRoom = (name: string, maxPlayerCount: number) => {
    return apiRequest<JoinCreateRoomResponse>(
        `${API}/api/rooms/create`,
        {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ name, maxPlayerCount }),
        },
        "Room created successfully!"
    );
};

export const loadRoomPlayerInfo = (code: string) => {
    return apiRequest<PlayerInfoResponse[]>(
        `${API}/api/rooms/${code}/getPlayerInfo`
    );
};

export const loadObjectives = (code: string, playerId: number) => {
    return apiRequest<NemesisPlayerObjective[]>(
        `${API}/api/rooms/${code}/objectives?playerId=${playerId}`
    );
};

export const startGame = (code: string) => {
    return apiRequest<object>(
        `${API}/api/rooms/${code}/start`,
        { method: "POST" }
    );
};

export const selectObjective = (code: string, playerId: number, objectiveId: number) => {
    return apiRequest<PlayerInfoResponse[]>(
        `${API}/api/rooms/${code}/selectObjective`,
        {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ playerId, objectiveId }),
        },
        "Objective selected successfully!"
    );
};