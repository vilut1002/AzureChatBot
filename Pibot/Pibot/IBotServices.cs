// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.AI.Luis;

namespace Pibot
{
    public interface IBotServices
    {
        LuisRecognizer Dispatch { get; }
    }
}
