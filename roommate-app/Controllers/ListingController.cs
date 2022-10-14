﻿using Microsoft.AspNetCore.Mvc;
using roommate_app.Models;
using roommate_app.Other.ListingComparers;
using System.Text.Json;

namespace roommate_app.Controllers;

[ApiController]
[Route("[controller]")]
public class ListingController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public ListingController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    private List<Listing> LoadJson()
    {
        using StreamReader r = new StreamReader("Data/listings.json");
        string json = r.ReadToEnd();
        List<Listing> listings = JsonSerializer.Deserialize<List<Listing>>(json);
        return listings;
      
    }

    [HttpPost]
    [Route("sort")]
    public ActionResult GetSortedListings([FromBody] SortListingsRequestBody body)
    {
        var existingListings = LoadJson();

        var factory = new ListingComparerFactory();
        var comparer = factory.GetComparer(sortMode: body.Sort, city: body.City);

        existingListings.Sort(comparer);

        return base.Ok(existingListings);
    }

    [HttpPost]
    public ActionResult Submit([FromBody] Listing listing)
    {
        List<Listing> existingListings = LoadJson();
        existingListings.Add(listing);
        string json = JsonSerializer.Serialize(existingListings);
        using StreamWriter tsw = new StreamWriter("Data/listings.json", false);
        tsw.WriteLine(json);
        tsw.Close();

        return base.Ok(existingListings);
    }
}
