import { SafeUrl } from "@angular/platform-browser";

export interface Comment{
    profilePhoto: SafeUrl | null;
    username: string;
    firstname: string;
    lastname: string;
    comment: string;
    addedAt: string;
}