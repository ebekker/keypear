// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

global using System.Text;
global using System.Text.Json;
global using System.Numerics;
global using System.Security.Cryptography;

global using Sodium;

global using MessagePack;
global using MPObjectAttribute = MessagePack.MessagePackObjectAttribute;
global using MPKeyAttribute = MessagePack.KeyAttribute;
global using MPIgnoreAttribute = MessagePack.IgnoreMemberAttribute;
