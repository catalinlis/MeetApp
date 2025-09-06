import { MONTHS } from "../constants/data-constants";
import { formatDate } from "@angular/common";

export class DateProcessing{

    static getDateDDMMYYYY(dateObject: Date): string{
        
        const date = new Date(dateObject);
        const day = String(date.getDate());
        const month = MONTHS[date.getMonth()] 
        const year = String(date.getFullYear());
            
        return `${day} ${month} ${year}`;
    }

    static getTimeHHMM(dateObject: Date): string{
        const date = new Date(dateObject);
        const hours = String(date.getHours()).padStart(2, "0");
        const minutes = String(date.getMinutes()).padStart(2, "0");

        return `${hours}:${minutes}`;
    }

    static differenceBetweenDates(firstDate: Date, secondDate: Date, difference: number): boolean{
        const date1 = new Date(firstDate);
        const date2 = new Date(secondDate);
        const diffMs = Math.abs(date1.getTime() - date2.getTime());

        return diffMs > difference * 1000 ? true : false;
    }

    static isToday(date: Date): boolean{

        const todayDate = new Date();
        const receivedDate = new Date(date);

        return this.getDateDDMMYYYY(todayDate) === this.getDateDDMMYYYY(receivedDate);
    }

    static formatPostDate(date: Date): string {
        const postDate = new Date(date);
        const now = new Date();

        const isToday = postDate.toDateString() == now.toDateString();
        const diffInMs = now.getTime() - postDate.getTime();
        const diffInDays = diffInMs / (1000 * 60 * 60 * 24);

        const sameYear = postDate.getFullYear() === now.getFullYear();

        if(isToday)
            return formatDate(postDate, 'HH:mm', 'en-US');

        if(diffInDays < 7)
            return formatDate(postDate, 'EEE HH:mm', 'en-US');

        if(sameYear)
            return formatDate(postDate, 'd MMM', 'en-US');

        return formatDate(postDate, 'd MMM yyyy', 'en-US');

    }
}