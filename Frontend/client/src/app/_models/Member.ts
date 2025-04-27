import { SafeUrl } from "@angular/platform-browser";

export interface Member{
    firstname: string;
    lastname: string;
    username: string;
    country: string;
    city: string;
    profilePhoto: string;
    safeImageUrl?: SafeUrl | null; // Optional for sanitized image URLs
    imageLoaded?: boolean;
}