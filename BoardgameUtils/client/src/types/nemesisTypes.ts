export const ObjectiveType = {
    Cooperative: 1,
    Selfish: 2,
}

export type NemesisPlayerObjective = {
    id: number;
    title: string;
    type: number;
};
