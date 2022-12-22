﻿/*
 * Copyright 2021-2022 nurhafiz@hotmail.sg
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

namespace Toimik.SitemapsProtocol;

using System;
using System.Collections.Generic;

public class EntryComparer : IEqualityComparer<IEntry>
{
    public EntryComparer()
    {
    }

    public bool Equals(IEntry? entry, IEntry? otherEntry)
    {
        var isEquals = entry != null
            && otherEntry != null
            && entry.Location != null
            && entry.Location.Equals(otherEntry.Location, StringComparison.OrdinalIgnoreCase);
        return isEquals;
    }

    public int GetHashCode(IEntry entry)
    {
        var location = entry.Location;
        location ??= string.Empty;
        var hashCode = location.GetHashCode();
        return hashCode;
    }
}