﻿using System;
using Akka.Actor;
using Xunit;

namespace Akka.Tests.Actor
{
    public partial class ReceiveActorTests
    {
        [Fact]
        public void Given_actor_When_it_restarts_Then_uses_the_handler()
        {
            //Given
            var system = ActorSystem.Create("test");
            var actor = system.ActorOf<CrashActor>("crash");
            
            //When
            actor.Tell("CRASH");

            //Then
            actor.Tell("hello", TestActor);
            ExpectMsg((object) "1:hello");
        }

        [Fact]
        public void Given_actor_that_has_replaced_its_initial_handler_When_it_restarts_Then_uses_the_initial_handler()
        {
            //Given
            var system = ActorSystem.Create("test");
            var actor = system.ActorOf<CrashActor>("crash");
            actor.Tell("BECOME-DISCARD");

            //When
            actor.Tell("CRASH", TestActor);

            //Then
            actor.Tell("hello", TestActor);
            ExpectMsg((object) "1:hello");
        }


        [Fact]
        public void Given_actor_that_has_pushed_a_new_handler_When_it_restarts_Then_uses_the_initial_handler()
        {
            //Given
            var system = ActorSystem.Create("test");
            var actor = system.ActorOf<CrashActor>("crash");
            actor.Tell("BECOME");

            //When
            actor.Tell("CRASH", TestActor);

            //Then
            actor.Tell("hello", TestActor);
            ExpectMsg((object) "1:hello");
        }

        private class CrashActor : ReceiveActor
        {
            public CrashActor()
            {
                Receive<string>(s => s == "CRASH", s => { throw new Exception("Crash!"); });
                Receive<string>(s => s == "BECOME", _ => Become(State2, discardOld: false));
                Receive<string>(s => s == "BECOME-DISCARD", _ => Become(State2, discardOld: true));
                Receive<string>(s => Sender.Tell("1:"+s));
            }

            private void State2()
            {
                Receive<string>(s => s == "CRASH", s => { throw new Exception("Crash!"); });
                Receive<string>(s => Sender.Tell("2:" + s));
            }
        }
    }
}