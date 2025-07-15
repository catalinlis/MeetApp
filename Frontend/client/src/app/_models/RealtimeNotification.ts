export interface RealtimeNotification{
    id: number;
    type: string;
    message: string;
    senderFirstname: string;
    senderLastname: string;
    senderProfilePhoto: string;
    createdAt: Date;
}