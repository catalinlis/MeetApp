export class StringProcess{
    
    static capitalizeFirstLetter(word: string){
        return word.charAt(0).toUpperCase() + word.slice(1);
    }
}