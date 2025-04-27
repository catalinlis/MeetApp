using API.Entities;
using Microsoft.AspNetCore.Identity;

namespace API.Data;

public class SeedData{

    public static async Task SeedInterest(DataContext context){

        if(!context.Interests.Any()){

            var interests = new List<Interest>{
                new Interest{ InterestKey = "programming", InterestName = "Programming" },
                new Interest{ InterestKey = "law", InterestName = "Law" },
                new Interest{ InterestKey = "sport", InterestName = "Sport" },
                new Interest{ InterestKey = "medicine", InterestName = "Medicine" },
                new Interest{ InterestKey = "health-and-beauty", InterestName = "Health&Beauty" },
                new Interest{ InterestKey = "animals", InterestName = "Animals" },
                new Interest{ InterestKey = "nutrition", InterestName = "Nutrition" },
                new Interest{ InterestKey = "architecture", InterestName = "Architecture" },
                new Interest{ InterestKey = "bussiness", InterestName = "Bussiness" },
                new Interest{ InterestKey = "fashion", InterestName = "Fashion" },
                new Interest{ InterestKey = "design", InterestName = "Design" },
                new Interest{ InterestKey = "lecture", InterestName = "Lecture" },
                new Interest{ InterestKey = "gaming", InterestName = "Gaming" },
                new Interest{ InterestKey = "music", InterestName = "Music" },
                new Interest{ InterestKey = "cooking", InterestName = "Cooking" },
                new Interest{ InterestKey = "religion", InterestName = "Religion" },
                new Interest{ InterestKey = "dancing", InterestName = "Dancing" },
                new Interest{ InterestKey = "chemistry", InterestName = "Chemistry" },
                new Interest{ InterestKey = "astronomy", InterestName = "Astronomy" },
                new Interest{ InterestKey = "computer-science", InterestName = "Computer Science" },
                new Interest{ InterestKey = "technology", InterestName = "Technology" }
            };

            foreach(var interest in interests){
                context.Interests.Add(interest);
            }

            await context.SaveChangesAsync();
        }
    }

}