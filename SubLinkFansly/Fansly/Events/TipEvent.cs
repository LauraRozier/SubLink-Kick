﻿using System;
using System.Text.Json.Serialization;

namespace xyz.yewnyx.SubLink.Fansly.Events;

public sealed class TipEvent {
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("chatRoomId")]
    public string ChatRoomId { get; set; } = string.Empty;

    [JsonPropertyName("senderId")]
    public string SenderId { get; set; } = string.Empty;

    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    [JsonPropertyName("displayname")]
    public string Displayname { get; set; } = string.Empty;

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    [JsonPropertyName("amount")]
    public float Amount { get; set; } = 0f;

    [JsonPropertyName("centAmount")]
    public int CentAmount { get; set; } = 0;

    [JsonPropertyName("createdAt")]
    public long CreatedAt { get; set; } = 0;

    public TipEvent() { }

    public TipEvent(string id, string chatRoomId, string senderId, string username, string displayname, string content, uint rawAmount, long createdAt) {
        Id = id;
        ChatRoomId = chatRoomId;
        SenderId = senderId;
        Username = username;
        Displayname = displayname;
        Content = content;
        Amount = (float)Math.Round(rawAmount / 1000d, 2, MidpointRounding.ToZero);
        CentAmount = (int)Math.Round(rawAmount / 10d, 0, MidpointRounding.ToZero);
        CreatedAt = createdAt;
    }

    public TipEvent(string id, string chatRoomId, string senderId, string username, string displayname, string content, float amount, int centAmount, long createdAt) {
        Id = id;
        ChatRoomId = chatRoomId;
        SenderId = senderId;
        Username = username;
        Displayname = displayname;
        Content = content;
        Amount = amount;
        CentAmount = centAmount;
        CreatedAt = createdAt;
    }
}
