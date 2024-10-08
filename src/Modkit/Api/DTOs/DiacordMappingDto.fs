﻿namespace Modkit.Api.DTOs

open Modkit.Api.Models
open System.Collections.Generic
open System.Text.Json.Serialization

type DiacordMappingDto = {
    [<JsonPropertyName "guild_id">] GuildId: string
    [<JsonPropertyName "mappings">] Mappings: IDictionary<string, string>
}
with
    static member from (model: DiacordMapping) = {
        GuildId = model.GuildId;
        Mappings = model.Mappings;
    }
