using System;
using UnityEngine;
using Verse;

namespace RERimhazard
{
    [StaticConstructorOnStartup]
    public static class TexButton
    {
        public static readonly Texture2D Combine = ContentFinder<Texture2D>.Get("UI/RECombine", true);

    }
}