using Microsoft.EntityFrameworkCore;
using roommate_app.Data;
using roommate_app.Models;
using roommate_app.Other.WebSocket;
using System.Diagnostics.CodeAnalysis;

namespace roommate_app.Services;

public interface IListingService
{
    IList<Listing> GetByUserId(int id);
    Task UpdateAsync(int id, Listing listing);
    List<Listing> Filter(int lowPrice, int highPrice, string city, int count);
}

public class ListingService : IListingService
{
    private Lazy<List<Listing>> _listings => new Lazy<List<Listing>>(() => _context.Listings.ToList());
    private readonly ApplicationDbContext _context;

    public delegate void ListingFeedUpdatedEventHandler(object source, EventArgs e);
    public event ListingFeedUpdatedEventHandler ListingFeedUpdated;

    public ListingService(ApplicationDbContext context)
    {
        _context = context;
        ListingFeedUpdated += PusherChannel.OnListingFeedUpdated;
    }

    public IList<Listing> GetByUserId(int id)
    {
        IList<Listing> userListings = new List<Listing>();
        IList<Listing> existingListings = new List<Listing>((IEnumerable<Listing>)_listings.Value);
        for (int i = 0; i < existingListings.Count; i++)
        {
            if (existingListings[i].UserId == id)
            {
                userListings.Add(existingListings[i]);
            }
        }
        return userListings;
    }
    public async Task UpdateAsync(int id, Listing listing)
    {
        var lst = _context.Listings.Where(l => l.Id == id).First();
        lst.Phone = listing.Phone;
        lst.RoommateCount = listing.RoommateCount;
        lst.MaxPrice = listing.MaxPrice;
        lst.ExtraComment = listing.ExtraComment;
        await _context.SaveChangesAsync();
        OnListingFeedUpdated();
    }

    public List<Listing> Filter(int lowPrice = 0, int highPrice = 100000, string city = "Vilnius", int count = 1)
    {
        int low = lowPrice;
        int high = highPrice;
        string realCity = city;
        int number = count;

        var listings = _context.Listings
            .Where(l => l.MaxPrice >= low && l.MaxPrice <= high && l.City == realCity && l.RoommateCount == number)
            .ToList();

        return listings;
    }

    protected virtual void OnListingFeedUpdated()
    {
        if (ListingFeedUpdated != null)
        {
            ListingFeedUpdated(this, EventArgs.Empty);
        }
    }

}