export interface Label {
    labelId: number;
    boardId: number;
    name: string;
    color: string;
    createdAt: Date;
}

export interface CreateLabelDto {
    boardId: number;
    name: string;
    color: string;
}

export interface Checklist {
    checklistId: number;
    cardId: number;
    title: string;
    position: number;
    createdAt: Date;
    items: ChecklistItem[];
}

export interface ChecklistItem {
    itemId: number;
    checklistId: number;
    text: string;
    isCompleted: boolean;
    assigneeId?: number;
    dueDate?: Date;
}

export interface CreateChecklistDto {
    cardId: number;
    title: string;
    position: number;
}
