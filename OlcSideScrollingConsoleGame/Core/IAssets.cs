#nullable enable
using PixelEngine;
using OlcSideScrollingConsoleGame.Models;
using OlcSideScrollingConsoleGame.Models.Items;

namespace OlcSideScrollingConsoleGame.Core
{
    /// <summary>
    /// Abstraherar åtkomst till speltillgångar (sprites, items, kartdata).
    /// Implementeras av Aggregate i produktion och FakeAssets i tester.
    /// </summary>
    public interface IAssets
    {
        /// <summary>Hämtar en sprite med angivet namn. Returnerar null om den inte finns.</summary>
        Sprite? GetSprite(string name);

        /// <summary>Hämtar ett item-objekt med angivet namn. Returnerar null om det inte finns.</summary>
        Item? GetItem(string name);

        /// <summary>Hämtar kartdata (LevelObj) med angivet namn. Returnerar null om den inte finns.</summary>
        LevelObj? GetMapData(string name);
    }
}
