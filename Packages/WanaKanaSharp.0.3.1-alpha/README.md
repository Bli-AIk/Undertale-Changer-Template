<div align="center">
    <h1>ワナカナ &lt;--&gt; WanaKanaSharp &lt;--&gt; わなかな</h1>
    <h4>A .NET library for checking and converting between Hiragana, Katakana and Romaji</h4>
    <h6>This project is a port of :crocodile: <a href="https://github.com/WaniKani/WanaKana">WanaKana</a> :crab: for the .NET platform</h6>
</div>

## Status

<div align="center">
    <table>
        <tbody align="center">
            <tr>
                <th>
                    <strong width="1000px">Travis CI</strong>
                </th>
                <th>
                    <strong width="1000px">AppVeyor</strong>
                </th>
            </tr>
            <tr>
                <td>
                    <a href="https://travis-ci.org/caguiclajmg/WanaKanaSharp" width="50%">
                        <img src="https://img.shields.io/travis/caguiclajmg/WanaKanaSharp.svg" />
                    </a>
                </td>
                <td>
                    <a href="https://ci.appveyor.com/project/caguiclajmg/wanakanasharp" width="50%">
                        <div>
                            <img src="https://img.shields.io/appveyor/ci/caguiclajmg/WanaKanaSharp.svg" />
                        </div>
                        <div>
                            <img src="https://img.shields.io/appveyor/tests/caguiclajmg/WanaKanaSharp.svg" />
                        </div>
                    </a>
                </td>
            </tr>
        </tbody>
    </table>
</div>

> **Warning** Pre-release versions (<1.0) are experimental, expect API breakage!

## Usage

Pre-built packages are available from [NuGet](https://www.nuget.org/packages/WanaKanaSharp/)

### Visual Studio

Search for WanaKanaSharp on NuGet Package Manager

![NuGet Package Manager](docs/visualstudio-package.png)

### .NET Core

Navigate to your project's directory and do: `dotnet add package WanaKanaSharp`

## Examples

```cs
    // Utility functions
    WanaKana.IsRomaji("hello"); // true
    WanaKana.IsHiragana("こんにちは"); // true
    WanaKana.IsKatakana("テレビ"); // true
    WanaKana.IsKana("これはキュートです"); // true
    WanaKana.IsKanji("日本語"); // true

    // Romaji conversion
    RomajiConverter romajiConverter = new HepburnRomajiConverter();
    romajiConverter.ToRomaji("ひらがな", false, null); // hiragana
    romajiConverter.ToRomaji("カタカナ", false, null); // katakana

    // Kana conversion
    KanaConverter kanaConverter = new DefaultKanaConverter();
    kanaConverter.ToKana("hiragana", null); // ひらがな
    kanaConverter.ToKana("KATAKANA", null); // カタカナ
```
