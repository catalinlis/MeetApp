namespace NotificationQueue.Helpers;

public static class ObjectPrinter{

    public static void PrintProperties(object obj){
        var type = obj.GetType();
        var props = type.GetProperties();

        foreach(var prop in props){
            Console.WriteLine($"{prop.Name}: {prop.GetValue(obj)}");
        }
    }

}