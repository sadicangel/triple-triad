using LiteDB;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using TripleTriad.Models;
using TripleTriad.Repositories;
using Xunit;

namespace TripleTriad.Tests;

public class NaturalStringComparer : IComparer<string>
{
    [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
    private static extern int StrCmpLogicalW(string psz1, string psz2);

    public int Compare(string? a, string? b)
    {
        if (a is null)
            return b is null ? 0 : 1;
        if (b is null)
            return -1;
        return StrCmpLogicalW(a, b);
    }
}

public class UnitTest1
{
    public static string GenerateId(int edition, int tier, int number, int version)
    {
        return $"CardE{edition:D2}T{tier:D2}N{number:D3}V{version:D1}";
    }

    [Fact]
    public void Unzip_Cards()
    {
        const string source = @"D:\Development\TripleTriad_Old\ZIP";
        const string target = @"D:\Development\triple-triad\TripleTriad.Shared\Assets\";

        int cardNo = 1;
        foreach (var batch in Directory.EnumerateFiles(source, "*.png").OrderBy(f => f, new NaturalStringComparer()))
        {
            using var img = Image.Load(batch);
            const int w = 256, h = 256;
            for (int v = 0; v < 4 * h; v += h)
            {
                for (int u = 0; u < w * 2; u += w, ++cardNo)
                {
                    var rect = new Rectangle(u, v, w, h);
                    var id = GenerateId(1, 1 + (cardNo - 1) / 11, cardNo, 1);
                    img.Clone(a => a.Crop(rect)).Save(Path.Combine(target, $"{id}.png"));
                }
            }
        }
    }

    [Fact]
    public void Cards_Bson()
    {
        using var liteDb = new LiteDatabase(@"Assets\Cards.db", BsonMapper.Global.UseCamelCase());

        var repository = new CardRepository(liteDb);
        var cardsJson = System.Text.Json.JsonSerializer.Serialize(repository.FindAll(),
            new JsonSerializerOptions
            {
                WriteIndented = true,
                Converters = {
                    new JsonStringEnumConverter()
                }
            });
        File.WriteAllText(@"D:\Development\triple-triad\TripleTriad.Shared\cards.json", cardsJson);
    }

    [Fact]
    public void Cards_Json()
    {
        var cards = System.Text.Json.JsonSerializer.Deserialize<Card[]>(File.ReadAllText(@"D:\Development\TripleTriad_Old\cards.json"))!;

        var setVersion = typeof(Card).GetProperty(nameof(Card.Version))!.GetSetMethod()!.CreateDelegate<Action<Card, int>>();
        var setId = typeof(Card).GetProperty(nameof(Card.Id))!.GetSetMethod()!.CreateDelegate<Action<Card, string>>();

        var bsonMapper = BsonMapper.Global.UseCamelCase();
        using var liteDb = new LiteDatabase(@"D:\Development\triple-triad\TripleTriad.Shared\Images\cards.db", bsonMapper);

        var repository = new CardRepository(liteDb);

        foreach (var card in cards)
        {
            setVersion.Invoke(card, 1);
            setId.Invoke(card, GenerateId(card.Edition, card.Tier, card.Number, card.Version));
            repository.Insert(card);
        }
    }
}