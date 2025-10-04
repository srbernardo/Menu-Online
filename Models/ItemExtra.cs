using System;

namespace MenuOnline.Models;

public class ItemExtra
{
  public int ItemId { get; set; }
  public Item? Item { get; set; }
  public int ExtraId { get; set; }
  public Extra? Extra { get; set; }
}
