using System.Collections.Generic;

namespace WeddingPlanner.Models
{
  public class Dashboard
  {
    public List<Wedding> Weddings { get; set; }
    public List<Guest> Guests { get; set; }
    public User User { get; set; }
  }
}