// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com



// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InventoryResult.cs" company="SyndicatedLife">
//   Copyright© 2007 - 2020 Ryan Wilson &amp;lt;syndicated.life@gmail.com&amp;gt; (https://syndicated.life/)
//   Licensed under the MIT license. See LICENSE.md in the solution root for full license information.
// </copyright>
// <summary>
//   InventoryResult.cs Implementation
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sharlayan.Models.ReadResults {
    using System.Collections.Generic;

    using Sharlayan.Core;

    public class InventoryResult {
        public List<InventoryContainer> InventoryContainers { get; } = new List<InventoryContainer>();
    }
}