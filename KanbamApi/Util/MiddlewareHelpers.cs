using KanbamApi.Models;

namespace KanbamApi.Util;

public class MiddlewareHelpers
{
    public static List<string> RemoveDuplicate(List<string> listOfData) =>
        listOfData.Distinct().ToList();

    public static Visitor UpdateVisitor(Visitor visitorDetail, List<Visitor> visitor)
    {
        visitorDetail.Id = visitor[0].Id;

        visitorDetail.Timestamp = [.. visitor[0].Timestamp, .. visitorDetail.Timestamp];

        visitorDetail.HttpMethod = RemoveDuplicate(
            [.. visitor[0].HttpMethod, .. visitorDetail.HttpMethod]
        );

        visitorDetail.Path = RemoveDuplicate([.. visitor[0].Path, .. visitorDetail.Path]);

        visitorDetail.UserAgent = RemoveDuplicate(
            [.. visitor[0].UserAgent, .. visitorDetail.UserAgent]
        );

        return visitorDetail;
    }
}
