using UniRx;
using System;
using UnityEngine;

public class EventAggregator
{
    static IMessageBroker messageBroker;
    static IMessageBroker MessageBroker => messageBroker ??  (messageBroker = new MessageBroker());

    public static void Publish<T>(T message)
    { MessageBroker.Publish(message); }

    public static IObservable<T> OnEvent<T>()
    { return MessageBroker.Receive<T>(); }
}

public class OnUIClick
{
    public Vector3 WorldPos;
    public OnUIClick(Vector3 worldPos) {
        WorldPos = worldPos;
    }
}

public class OnMapChange { }