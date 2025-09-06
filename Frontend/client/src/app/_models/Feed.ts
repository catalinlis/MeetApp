import { Interest } from "./Interest";

export interface Feed{
    type: string;
    content: string;
    imageUrl: string;
    createdAt: Date;
    author: string;
    likesCount: number;
    likedByMe: boolean;
    commentsCount: number;
    interests: Interest[];
    postId: number;
}