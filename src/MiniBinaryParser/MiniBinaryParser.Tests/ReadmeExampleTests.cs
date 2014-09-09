﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace MiniBinaryParser.Tests
{
    [TestClass]
    public class ReadmeExampleTests
    {
        internal class User
        {
            public Guid UserId { get; set; }
            public DateTime BirthDate { get; set; }
            public uint NumberOfFriends { get; set; }
        }

        [TestMethod]
        public void TestBinaryMatch()
        {
            byte[] sequence = new byte[] { 0x00, 0x01, 0x02, 0x03  };
            Match match = sequence.Parse(Endian.Big, (byte)0x01, (byte)0x02);

            byte[] matchedSequence = match.MatchedBytes;
            byte[] unmatchedSequence = match.UnmatchedBytes;
            CollectionAssert.AreEqual(new byte[] { 0x01, 0x02 }, matchedSequence);
            CollectionAssert.AreEqual(new byte[] { 0x00, 0x03 }, unmatchedSequence);
        }

        [TestMethod]
        public void TestSimpleTLV()
        {
            byte[] sequence = new byte[] { 0x00, 0x00, 0xAA, 0xAF, 0x00, 0x04, 0x01, 0x02, 0x03, 0x04, 0x05  };
            short length = 0;
            byte[] data = null;
 
            Match m = sequence.Parse(Endian.Big, 
                0x0000AAAF,                                 // type
                From.Int16((len) => length = len),          // length
                From.Bytes(() => length, (bt) => data = bt) // value
                );

            CollectionAssert.AreEqual(new byte[] { 0x01, 0x02, 0x03, 0x04 }, data);
        }


        [TestMethod]
        public void TestMultipleTypes()
        {
            byte[] sequence = new byte[] { 
                0x00, 0x00, 0xFF, 0xAF,                                                                         // type
                0xA0, 0xA1, 0xA2, 0xA3, 0xA4, 0xA5, 0xA6, 0xA7, 0xA8, 0xA9, 0xAA, 0xAB, 0xAC, 0xAD, 0xAE, 0xAF, // Guid RFC4122
                0x00, 0x00, 0x01, 0x20,                                                                         // Unsigned int
                0x0D, 0x41, 0x75, 0x00                                                                          //  Unix Epoch DateTime 
            };
            User user = new User();
            Match m = sequence.Parse(Endian.Big,
                0x0000FFAF,                                                      // Type
                From.Guid((userId) => user.UserId = userId),                    // Guid RFC4122
                From.UInt32((friends) => user.NumberOfFriends = friends),       // Unsigned int
                From.EpochTimestamp((birthDate) => user.BirthDate = birthDate)  // Unix Epoch DateTime
            );

            Assert.AreEqual(Guid.Parse("a0a1a2a3-a4a5-a6a7-a8a9-aaabacadaeaf"), user.UserId);
            Assert.AreEqual((uint)288, user.NumberOfFriends);
            Assert.AreEqual(new DateTime(1977,01,18), user.BirthDate);

        }
    }
}