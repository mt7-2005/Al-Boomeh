using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

var httpClient = new HttpClient();

var baseUrl = "https://localhost:7027/api/Orders";

httpClient.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue(
        "Bearer",
        "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6Ijk3NmMxM2MxLTZhMGItNGUzNy1hZWQzLTQzYmE0OTQ0ZTkwOSIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IkN1c3RvbWVyIiwiY3VzdG9tZXJJZCI6IjUiLCJleHAiOjE3ODk1OTQyNjksImlzcyI6IkFsLUJvb21laEFQSSIsImF1ZCI6IkFsLUJvb21laEFQSVVzZXJzIn0.MXqgVglT4759Yp2TTyHlIUlh_rtHhYJ2retFejRtdpg"
    );

const int numberOfPairs = 20;

for (int pairNumber = 1; pairNumber <= numberOfPairs; pairNumber++)
{
    Console.WriteLine();
    Console.WriteLine($"========== PAIR {pairNumber} ==========");

    // =========================================================
    // 1. Create Order A
    //    Product 5 -> Product 6
    // =========================================================

    var orderAId = await CreateOrder(
        productFirst: 40001,
        productSecond: 40002,
        pairNumber: pairNumber,
        orderName: "A"
    );

    // =========================================================
    // 2. Create Order B
    //    Product 6 -> Product 5
    // =========================================================

    var orderBId = await CreateOrder(
        productFirst: 6,
        productSecond: 5,
        pairNumber: pairNumber,
        orderName: "B"
    );

    if (orderAId == null || orderBId == null)
    {
        Console.WriteLine("Could not create both orders. Skipping pair.");
        continue;
    }

    Console.WriteLine(
        $"Created Pair {pairNumber}: " +
        $"Order A = {orderAId}, Order B = {orderBId}"
    );

    // =========================================================
    // 3. Place both orders at the same time
    // =========================================================

    var taskA = PlaceOrder(orderAId.Value, pairNumber, "A");
    var taskB = PlaceOrder(orderBId.Value, pairNumber, "B");

    await Task.WhenAll(taskA, taskB);

    Console.WriteLine($"========== END PAIR {pairNumber} ==========");

    // Small delay between pairs
    await Task.Delay(100);
}

Console.WriteLine();
Console.WriteLine("All 20 pairs completed.");


// =============================================================
// CREATE ORDER
// =============================================================

async Task<int?> CreateOrder(
    int productFirst,
    int productSecond,
    int pairNumber,
    string orderName)
{
    var requestBody = new
    {
        customerId = 5,
        storeId = 1,

        orderLines = new[]
        {
            new
            {
                id = 0,
                customerId = 5,
                productId = productFirst,
                quantity = 1,
                notes = $"Deadlock Test Pair {pairNumber} Order {orderName}",
                extraId = 0
            },

            new
            {
                id = 0,
                customerId = 5,
                productId = productSecond,
                quantity = 1,
                notes = $"Deadlock Test Pair {pairNumber} Order {orderName}",
                extraId = 0
            }
        }
    };

    var json = JsonSerializer.Serialize(requestBody);

    using var request = new HttpRequestMessage(
        HttpMethod.Post,
        baseUrl
    );

    // Unique Idempotency-Key for every CREATE request
    request.Headers.Add(
        "Idempotency-Key",
        $"deadlock-test-create-{pairNumber}-{orderName}-{Guid.NewGuid()}"
    );

    request.Content = new StringContent(
        json,
        Encoding.UTF8,
        "application/json"
    );

    var response = await httpClient.SendAsync(request);

    var responseBody =
        await response.Content.ReadAsStringAsync();

    Console.WriteLine(
        $"CREATE {orderName} | " +
        $"{(int)response.StatusCode} {response.StatusCode}"
    );

    if (!response.IsSuccessStatusCode)
    {
        Console.WriteLine(responseBody);
        return null;
    }

    Console.WriteLine(
        $"CREATE {orderName} BODY: {responseBody}"
    );

    return ExtractOrderId(responseBody);
}


// =============================================================
// PLACE ORDER
// =============================================================

async Task PlaceOrder(
    int orderId,
    int pairNumber,
    string orderName)
{
    var url = $"{baseUrl}?id={orderId}";

    var requestBody = new
    {
        type = 0,
        paymentMethod = 0,
        customerId = 5,
        addressId = 7,

        storeNotes =
            $"Deadlock test Pair {pairNumber} Order {orderName}",

        latitude = "31.95",
        longitude = "35.91"
    };

    var json = JsonSerializer.Serialize(requestBody);

    using var request = new HttpRequestMessage(
        HttpMethod.Put,
        url
    );

    // IMPORTANT:
    // PlaceOrder does NOT require Idempotency-Key,
    // so we don't add one here.

    request.Content = new StringContent(
        json,
        Encoding.UTF8,
        "application/json"
    );

    try
    {
        var response = await httpClient.SendAsync(request);

        var responseBody =
            await response.Content.ReadAsStringAsync();

        Console.WriteLine(
            $"PLACE {orderName} | " +
            $"Order {orderId} | " +
            $"{(int)response.StatusCode} {response.StatusCode}"
        );

        Console.WriteLine(
            $"PLACE {orderName} BODY: {responseBody}"
        );
    }
    catch (Exception ex)
    {
        Console.WriteLine(
            $"PLACE {orderName} | " +
            $"Order {orderId} | EXCEPTION"
        );

        Console.WriteLine(ex);
    }
}


// =============================================================
// EXTRACT ORDER ID
// =============================================================

int? ExtractOrderId(string responseBody)
{
    try
    {
        using var document =
            JsonDocument.Parse(responseBody);

        var root = document.RootElement;

        // Case 1:
        // { "id": 123 }

        if (root.TryGetProperty("id", out var idProperty))
        {
            if (idProperty.TryGetInt32(out var id))
                return id;
        }

        // Case 2:
        // { "data": { "id": 123 } }

        if (root.TryGetProperty("data", out var dataProperty))
        {
            if (dataProperty.TryGetProperty("id", out var dataId))
            {
                if (dataId.TryGetInt32(out var id))
                    return id;
            }
        }

        Console.WriteLine(
            "Could not find Order ID in response."
        );

        return null;
    }
    catch (Exception ex)
    {
        Console.WriteLine(
            $"Failed to parse Order ID: {ex.Message}"
        );

        return null;
    }
}// refresh token

//var httpClient = new HttpClient();

//var url = "https://localhost:7027/api/Auth/refresh";
//var numberOfRequests = 2;


//httpClient.DefaultRequestHeaders.Authorization =
//    new AuthenticationHeaderValue("Bearer", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjQ4ZTNkZjVhLWFkMzYtNDY0My04NDY1LWVjYTQxMTQxNjQzMyIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IkFkbWluIiwiZXhwIjoxNzg5NTEzNTc4LCJpc3MiOiJBbC1Cb29tZWhBUEkiLCJhdWQiOiJBbC1Cb29tZWhBUElVc2VycyJ9.YgqcpQ1T3x_UKwSbBrNw19yeyAUTzsAfqUblQOCqOys");

//var tasks = Enumerable.Range(1, numberOfRequests)
//    .Select(async i =>
//    {
//        try
//        {
//            var requestBody = new
//            {
//                refreshToken = "mHzcpkZ1iiv/BDJAz/JB5EZmDbUM6XZiLJfe4ZzghyKhZ1GP8gSNGvVEtEtbopIMvDn6P4gt9dWGAhLX8R4/MA=="
//            }
//            ;

//            var json = JsonSerializer.Serialize(requestBody);

//            using var content = new StringContent(
//                json,
//                Encoding.UTF8,
//                "application/json");

//            var response = await httpClient.PostAsync(url, content);

//            var responseBody = await response.Content.ReadAsStringAsync();

//            Console.WriteLine(
//                $"Request {i}: {(int)response.StatusCode} {response.StatusCode}");
//        }
//        catch (Exception ex)
//        {
//            Console.WriteLine($"Request {i}: ERROR - {ex.Message}");
//        }
//    });

//await Task.WhenAll(tasks);

//Console.WriteLine("All requests completed.");


// verify 

//var client = new HttpClient
//{
//    BaseAddress = new Uri("https://localhost:7027")
//};

//string phone = "0794741703";
//string code = "508988";

//var tasks = Enumerable.Range(0, 2)
//    .Select(_ =>
//        client.PutAsync(
//            $"/api/Auth/Verify?phone={phone}&code={code}",
//            null));

//var responses = await Task.WhenAll(tasks);

//foreach (var response in responses)
//{
//    Console.WriteLine(
//        $"{(int)response.StatusCode} - {response.StatusCode}");
//}

// لل place order

//var httpClient = new HttpClient();

//var url = "https://localhost:7027/api/Orders";
//var numberOfRequests = 2;

//httpClient.DefaultRequestHeaders.Authorization =
//   new AuthenticationHeaderValue("Bearer", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6Ijk3NmMxM2MxLTZhMGItNGUzNy1hZWQzLTQzYmE0OTQ0ZTkwOSIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IkN1c3RvbWVyIiwiY3VzdG9tZXJJZCI6IjUiLCJleHAiOjE3ODk1OTA3MDYsImlzcyI6IkFsLUJvb21laEFQSSIsImF1ZCI6IkFsLUJvb21laEFQSVVzZXJzIn0.J9IpTVFM6_rgXX1NvDu0RfXkCeavBsLi6PAGOulyK6M");

//var orderIds = new List<int>
//{300472,
//300471

//};
//var tasks = orderIds.Select(async orderId =>
//{
//    var url = $"https://localhost:7027/api/Orders?id={orderId}";

//    var requestBody = new
//    {
//        type = 0,
//        paymentMethod = 0,
//        customerId = 5,
//        addressId = 7,
//        storeNotes = $"Race test sol 1- Order {orderId}",
//        latitude = "31.95",
//        longitude = "35.91",
//    };

//    var json = JsonSerializer.Serialize(requestBody);

//    using var content = new StringContent(
//        json,
//        Encoding.UTF8,
//        "application/json");

//    var response = await httpClient.PutAsync(url, content);

//    var responseBody = await response.Content.ReadAsStringAsync();

//    Console.WriteLine(
//        $"Order {orderId} | " +
//        $"{(int)response.StatusCode} {response.StatusCode}");
//});

//await Task.WhenAll(tasks);

//// لل create order

//var httpClient = new HttpClient();

//var url = "https://localhost:7027/api/Orders";
//var numberOfRequests = 2;


//httpClient.DefaultRequestHeaders.Authorization =
//    new AuthenticationHeaderValue("Bearer", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6Ijk3NmMxM2MxLTZhMGItNGUzNy1hZWQzLTQzYmE0OTQ0ZTkwOSIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IkN1c3RvbWVyIiwiY3VzdG9tZXJJZCI6IjUiLCJleHAiOjE3ODk1ODMxMzYsImlzcyI6IkFsLUJvb21laEFQSSIsImF1ZCI6IkFsLUJvb21laEFQSVVzZXJzIn0.ClZffA90ib_tVO4ZkLYKC7K55Cx5N1VJm-3oppN4KAY");

//var idempotencyKey = "race-test-123";

//var tasks = Enumerable.Range(1, numberOfRequests)
//    .Select(async i =>
//    {
//        try
//        {
//            var requestBody = new
//            {
//                customerId = 5,
//                storeId = 1,
//                orderLines = new[]
//                {
//                    new
//                    {
//                        id = 0,
//                        customerId = 5,
//                        productId = 5,
//                        quantity = 1,
//                        notes = "test-double click",
//                        extraId = 0
//                    }
//                }
//            };

//            var json = JsonSerializer.Serialize(requestBody);

//            using var request = new HttpRequestMessage(
//                HttpMethod.Post,
//                url);

//            request.Headers.Add(
//                "Idempotency-Key",
//                idempotencyKey);

//            request.Content = new StringContent(
//                json,
//                Encoding.UTF8,
//                "application/json");

//            var response = await httpClient.SendAsync(request);

//            Console.WriteLine(
//                $"Request {i}: {(int)response.StatusCode} {response.StatusCode}");
//        }
//        catch (Exception ex)
//        {
//            Console.WriteLine(
//                $"Request {i}: ERROR - {ex.Message}");
//        }
//    });

//await Task.WhenAll(tasks);

//Console.WriteLine("All requests completed.");