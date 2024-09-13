﻿namespace Modkit.Diacord.Core.Structures

type DefaultValueAttribute = System.ComponentModel.DefaultValueAttribute
open System.Text.Json.Serialization

type DiacordSettings = {
    [<JsonPropertyName("strict_roles")>]
    [<JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)>] // TODO: Test this
    [<DefaultValue(false)>]
    StrictRoles: bool

    [<JsonPropertyName("strict_emojis")>]
    [<JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)>] // TODO: Test this
    [<DefaultValue(false)>]
    StrictEmojis: bool

    [<JsonPropertyName("strict_stickers")>]
    [<JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)>] // TODO: Test this
    [<DefaultValue(false)>]
    StrictStickers: bool
}