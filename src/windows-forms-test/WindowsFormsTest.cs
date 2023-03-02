﻿// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric Engine contributors.
// Licensed under the MIT License. See LICENSE.txt in root directory.
// ***********************************************************************

using System.Windows.Forms;
using NUnit.Framework;

// Test which resolves issue #1203
namespace Test1
{
    [TestFixture]
    public class WindowsFormsTest
    {
        [Test]
        public void WithoutFramework()
        {
            Assert.Pass();
        }

        [Test]
        public void WithFramework()
        {
            var checkbox = new CheckBox();
            Assert.Pass();
        }
    }
}