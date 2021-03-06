﻿using NUnit.Framework;
using System;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;

namespace NonFactors.Mvc.Grid.Tests.Unit
{
    [TestFixture]
    public class BooleanFilterTests
    {
        private BooleanFilter<GridModel> filter;
        private IQueryable<GridModel> models;

        [SetUp]
        public void SetUp()
        {
            models = new[]
            {
                new GridModel(),
                new GridModel { IsChecked = true, NIsChecked = true },
                new GridModel{ IsChecked = false, NIsChecked = false }
            }.AsQueryable();

            Expression<Func<GridModel, Boolean>> expression = (model) => model.IsChecked;
            filter = new BooleanFilter<GridModel>();
            filter.FilteredExpression = expression;
        }

        #region Method: Process(IQueryable<T> items)

        [Test]
        public void Process_OnInvalidBooleanValueReturnsItems()
        {
            filter.Value = "Test";

            IEnumerable actual = filter.Process(models);
            IEnumerable expected = models;

            Assert.AreSame(expected, actual);
        }

        [Test]
        [TestCase("true")]
        [TestCase("True")]
        [TestCase("TRUE")]
        public void Process_FiltersNullableUsingEqualsTrue(String value)
        {
            Expression<Func<GridModel, Boolean?>> expression = (model) => model.NIsChecked;
            filter.FilteredExpression = expression;
            filter.Value = value;

            IEnumerable expected = models.Where(model => model.NIsChecked == true);
            IEnumerable actual = filter.Process(models);

            CollectionAssert.AreEqual(expected, actual);
        }

        [Test]
        [TestCase("true")]
        [TestCase("True")]
        [TestCase("TRUE")]
        public void Process_FiltersUsingEqualsTrue(String value)
        {
            filter.Value = value;

            IEnumerable expected = models.Where(model => model.IsChecked);
            IEnumerable actual = filter.Process(models);

            CollectionAssert.AreEqual(expected, actual);
        }

        [Test]
        [TestCase("false")]
        [TestCase("False")]
        [TestCase("FALSE")]
        public void Process_FiltersNullableUsingEqualsFalse(String value)
        {
            Expression<Func<GridModel, Boolean?>> expression = (model) => model.NIsChecked;
            filter.FilteredExpression = expression;
            filter.Value = value;

            IEnumerable expected = models.Where(model => model.NIsChecked == false);
            IEnumerable actual = filter.Process(models);

            CollectionAssert.AreEqual(expected, actual);
        }

        [Test]
        [TestCase("false")]
        [TestCase("False")]
        [TestCase("FALSE")]
        public void Process_FiltersUsingEqualsFalse(String value)
        {
            filter.Value = value;

            IEnumerable expected = models.Where(model => !model.IsChecked);
            IEnumerable actual = filter.Process(models);

            CollectionAssert.AreEqual(expected, actual);
        }

        #endregion
    }
}
