﻿using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;

namespace NonFactors.Mvc.Grid.Tests.Unit
{
    [TestFixture]
    public class GridColumnTests
    {
        private GridColumn<GridModel, Object> column;
        private IGridFilters oldFilters;
        private IGrid<GridModel> grid;

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            oldFilters = MvcGrid.Filters;
        }

        [SetUp]
        public void SetUp()
        {
            grid = Substitute.For<IGrid<GridModel>>();
            grid.Query = new NameValueCollection();
            grid.Name = "Grid";

            column = new GridColumn<GridModel, Object>(grid, model => model.Name);

            MvcGrid.Filters = Substitute.For<IGridFilters>();
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            MvcGrid.Filters = oldFilters;
        }

        #region Property: SortOrder

        [Test]
        public void SortOrder_DoesNotChangeSortOrderAfterItsSet()
        {
            grid.Query = HttpUtility.ParseQueryString("Grid-Sort=Name&Grid-Order=Asc");

            column.SortOrder = null;

            Assert.IsNull(column.SortOrder);
        }

        [Test]
        [TestCase("Grid-Sort=Name&Grid-Order=", "Name", GridSortOrder.Desc, null)]
        [TestCase("Grid-Order=Desc", null, GridSortOrder.Asc, GridSortOrder.Desc)]
        [TestCase("Grid-Sort=Name&Grid-Order=Asc", "Name", GridSortOrder.Desc, GridSortOrder.Asc)]
        [TestCase("Grid-Sort=Name&Grid-Order=Desc", "Name", GridSortOrder.Asc, GridSortOrder.Desc)]
        public void SortOrder_GetsSortOrderFromQuery(String query, String name, GridSortOrder? initialOrder, GridSortOrder? expected)
        {
            grid.Query = HttpUtility.ParseQueryString(query);
            column.InitialSortOrder = initialOrder;
            column.Name = name;

            GridSortOrder? actual = column.SortOrder;

            Assert.AreEqual(expected, actual);
        }

        [Test]
        [TestCase("Grid-Sort=Name&Grid-Order=", "Grid-Sort=Name&Grid-Order=Desc", null)]
        [TestCase("Grid-Sort=Name&Grid-Order=Asc", "Grid-Sort=Name&Grid-Order=Desc", GridSortOrder.Asc)]
        [TestCase("Grid-Sort=Name&Grid-Order=Desc", "Grid-Sort=Name&Grid-Order=Asc", GridSortOrder.Desc)]
        public void SortOrder_DoesNotChangeSortOrderAfterFirstGet(String initialQuery, String changedQuery, GridSortOrder? expected)
        {
            grid.Query = HttpUtility.ParseQueryString(initialQuery);
            GridSortOrder? sortOrder = column.SortOrder;

            grid.Query = HttpUtility.ParseQueryString(changedQuery);

            GridSortOrder? actual = column.SortOrder;

            Assert.AreEqual(expected, actual);
        }

        [Test]
        [TestCase("Grid-Order=Desc", "", GridSortOrder.Asc)]
        [TestCase("Gride-Sort=Name&Grid-Order=Asc", "Name", GridSortOrder.Asc)]
        [TestCase("RGrid-Sort=Name&Grid-Order=Desc", "Name", GridSortOrder.Desc)]
        public void SortOrder_OnNotSpecifiedSortOrderSetsInitialSortOrder(String query, String name, GridSortOrder? initialOrder)
        {
            grid.Query = HttpUtility.ParseQueryString(query);
            column.InitialSortOrder = initialOrder;
            column.Name = name;

            GridSortOrder? actual = column.SortOrder;
            GridSortOrder? expected = initialOrder;

            Assert.AreEqual(expected, actual);
        }

        #endregion

        #region Property: Filter

        [Test]
        public void Filter_DoesNotChangeFilterAfterItsSet()
        {
            String query = "Grid-Name-Equals=Test";
            grid.Query = HttpUtility.ParseQueryString(query);
            IGridFilter<GridModel> filter = Substitute.For<IGridFilter<GridModel>>();
            MvcGrid.Filters.GetFilter(Arg.Any<IGridColumn<GridModel>>(), Arg.Any<String>(), Arg.Any<String>()).Returns(filter);

            column.Filter = null;

            Assert.IsNull(column.Filter);
        }

        [Test]
        [TestCase("Grid-Name-=", "", "")]
        [TestCase("Grid-Name-Equals=", "Equals", "")]
        [TestCase("Grid-Name-Equals=Test", "Equals", "Test")]
        [TestCase("Grid-Name-Equals=Test&Grid-Name-Equals=Value", "Equals", "Test")]
        [TestCase("Grid-Name-Equals=Test&Grid-Name-Contains=Value", "Equals", "Test")]
        public void Filter_SetsFilterThenItsNotSet(String query, String type, String value)
        {
            grid.Query = HttpUtility.ParseQueryString(query);
            IGridFilter<GridModel> filter = Substitute.For<IGridFilter<GridModel>>();
            MvcGrid.Filters.GetFilter(Arg.Any<IGridColumn<GridModel>>(), type, value).Returns(filter);

            Object actual = column.Filter;
            Object expected = filter;

            MvcGrid.Filters.Received().GetFilter(column, type, value);
            Assert.AreSame(expected, actual);
        }

        [Test]
        [TestCase("")]
        [TestCase("value")]
        [TestCase("=value")]
        [TestCase("Grid-Name=value")]
        [TestCase("RGrid-Name-Equals=value")]
        public void Filter_OnGridQueryWithoutFilterSetsFilterToNull(String query)
        {
            grid.Query = HttpUtility.ParseQueryString(query);
            IGridFilter<GridModel> filter = Substitute.For<IGridFilter<GridModel>>();
            MvcGrid.Filters.GetFilter(Arg.Any<IGridColumn<GridModel>>(), Arg.Any<String>(), Arg.Any<String>()).Returns(filter);

            Assert.IsNull(column.Filter);
        }

        [Test]
        public void Filter_DoesNotChangeFilterAfterFirstGet()
        {
            String query = "Grid-Name-Equals=Test";
            grid.Query = HttpUtility.ParseQueryString(query);
            IGridFilter<GridModel> filter = Substitute.For<IGridFilter<GridModel>>();
            MvcGrid.Filters.GetFilter(Arg.Any<IGridColumn<GridModel>>(), "Name", "Equals").Returns(filter);

            filter = Substitute.For<IGridFilter<GridModel>>();
            IGridFilter<GridModel> currentFilter = column.Filter;
            MvcGrid.Filters.GetFilter(Arg.Any<IGridColumn<GridModel>>(), Arg.Any<String>(), Arg.Any<String>()).Returns(filter);

            Object expected = currentFilter;
            Object actual = column.Filter;

            Assert.AreSame(expected, actual);
        }

        #endregion

        #region Property: FilterValue

        [Test]
        public void FilterValue_GetsValueFromFilter()
        {
            column.Filter = Substitute.For<IGridFilter<GridModel>>();
            column.Filter.Value = "Test";

            String expected = column.Filter.Value;
            String actual = column.FilterValue;

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void FilterValue_OnNullFilterReturnsNull()
        {
            column.Filter = null;

            Assert.IsNull(column.FilterValue);
        }

        [Test]
        public void FilterValue_SetsFilterValue()
        {
            column.Filter = Substitute.For<IGridFilter<GridModel>>();
            column.Filter.Value = null;
            column.FilterValue = "T";

            String actual = column.Filter.Value;
            String expected = "T";

            Assert.AreEqual(expected, actual);
        }

        #endregion

        #region Property: FilterType

        [Test]
        public void FilterType_GetsTypeFromFilter()
        {
            column.Filter = Substitute.For<IGridFilter<GridModel>>();
            column.Filter.Type = "Test";

            String expected = column.Filter.Type;
            String actual = column.FilterType;

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void FilterType_OnNullFilterReturnsNull()
        {
            column.Filter = null;

            Assert.IsNull(column.FilterType);
        }

        [Test]
        public void FilterType_SetsFilterType()
        {
            column.Filter = Substitute.For<IGridFilter<GridModel>>();
            column.Filter.Type = null;
            column.FilterType = "T";

            String actual = column.Filter.Type;
            String expected = "T";

            Assert.AreEqual(expected, actual);
        }

        #endregion

        #region Constructor: GridColumn(IGrid<T> grid, Expression<Func<T, TValue>> expression)

        [Test]
        public void GridColumn_SetsGrid()
        {
            IGrid actual = new GridColumn<GridModel, Object>(grid, model => model.Name).Grid;
            IGrid expected = grid;

            Assert.AreSame(expected, actual);
        }

        [Test]
        public void GridColumn_SetsIsEncodedToTrue()
        {
            column = new GridColumn<GridModel, Object>(grid, model => model.Name);

            Assert.IsTrue(column.IsEncoded);
        }

        [Test]
        public void GridColumn_SetsExpression()
        {
            Expression<Func<GridModel, String>> expected = (model) => model.Name;
            Expression<Func<GridModel, String>> actual = new GridColumn<GridModel, String>(grid, expected).Expression;

            Assert.AreSame(expected, actual);
        }

        [Test]
        public void AddProperty_SetsFilterNameAsNullForEnum()
        {
            AssertFilterNameFor(model => model.EnumField, null);
        }

        [Test]
        public void AddProperty_SetsFilterNameAsNumberForSByte()
        {
            AssertFilterNameFor(model => model.SByteField, "Number");
        }

        [Test]
        public void AddProperty_SetsFilterNameAsNumberForByte()
        {
            AssertFilterNameFor(model => model.ByteField, "Number");
        }

        [Test]
        public void AddProperty_SetsFilterNameAsNumberForInt16()
        {
            AssertFilterNameFor(model => model.Int16Field, "Number");
        }

        [Test]
        public void AddProperty_SetsFilterNameAsNumberForUInt16()
        {
            AssertFilterNameFor(model => model.UInt16Field, "Number");
        }

        [Test]
        public void AddProperty_SetsFilterNameAsNumberForInt32()
        {
            AssertFilterNameFor(model => model.Int32Field, "Number");
        }

        [Test]
        public void AddProperty_SetsFilterNameAsNumberForUInt32()
        {
            AssertFilterNameFor(model => model.UInt32Field, "Number");
        }

        [Test]
        public void AddProperty_SetsFilterNameAsNumberForInt64()
        {
            AssertFilterNameFor(model => model.Int64Field, "Number");
        }

        [Test]
        public void AddProperty_SetsFilterNameAsNumberForUInt64()
        {
            AssertFilterNameFor(model => model.UInt64Field, "Number");
        }

        [Test]
        public void AddProperty_SetsFilterNameAsNumberForSingle()
        {
            AssertFilterNameFor(model => model.SingleField, "Number");
        }

        [Test]
        public void AddProperty_SetsFilterNameAsNumberForDouble()
        {
            AssertFilterNameFor(model => model.DoubleField, "Number");
        }

        [Test]
        public void AddProperty_SetsFilterNameAsNumberForDecimal()
        {
            AssertFilterNameFor(model => model.DecimalField, "Number");
        }

        [Test]
        public void AddProperty_SetsFilterNameAsBooleanForBoolean()
        {
            AssertFilterNameFor(model => model.BooleanField, "Boolean");
        }

        [Test]
        public void AddProperty_SetsFilterNameAsDateCellForDateTime()
        {
            AssertFilterNameFor(model => model.DateTimeField, "Date");
        }

        [Test]
        public void AddProperty_SetsFilterNameAsNullForNullableEnum()
        {
            AssertFilterNameFor(model => model.NullableEnumField, null);
        }

        [Test]
        public void AddProperty_SetsFilterNameAsNumberForNullableSByte()
        {
            AssertFilterNameFor(model => model.NullableSByteField, "Number");
        }

        [Test]
        public void AddProperty_SetsFilterNameAsNumberForNullableByte()
        {
            AssertFilterNameFor(model => model.NullableByteField, "Number");
        }

        [Test]
        public void AddProperty_SetsFilterNameAsNumberForNullableInt16()
        {
            AssertFilterNameFor(model => model.NullableInt16Field, "Number");
        }

        [Test]
        public void AddProperty_SetsFilterNameAsNumberForNullableUInt16()
        {
            AssertFilterNameFor(model => model.NullableUInt16Field, "Number");
        }

        [Test]
        public void AddProperty_SetsFilterNameAsNumberForNullableInt32()
        {
            AssertFilterNameFor(model => model.NullableInt32Field, "Number");
        }

        [Test]
        public void AddProperty_SetsFilterNameAsNumberForNullableUInt32()
        {
            AssertFilterNameFor(model => model.NullableUInt32Field, "Number");
        }

        [Test]
        public void AddProperty_SetsFilterNameAsNumberForNullableInt64()
        {
            AssertFilterNameFor(model => model.NullableInt64Field, "Number");
        }

        [Test]
        public void AddProperty_SetsFilterNameAsNumberForNullableUInt64()
        {
            AssertFilterNameFor(model => model.NullableUInt64Field, "Number");
        }

        [Test]
        public void AddProperty_SetsFilterNameAsNumberForNullableSingle()
        {
            AssertFilterNameFor(model => model.NullableSingleField, "Number");
        }

        [Test]
        public void AddProperty_SetsFilterNameAsNumberForNullableDouble()
        {
            AssertFilterNameFor(model => model.NullableDoubleField, "Number");
        }

        [Test]
        public void AddProperty_SetsFilterNameAsNumberForNullableDecimal()
        {
            AssertFilterNameFor(model => model.NullableDecimalField, "Number");
        }

        [Test]
        public void AddProperty_SetsFilterNameAsBooleanForNullableBoolean()
        {
            AssertFilterNameFor(model => model.NullableBooleanField, "Boolean");
        }

        [Test]
        public void AddProperty_SetsFilterNameAsDateForNullableDateTime()
        {
            AssertFilterNameFor(model => model.NullableDateTimeField, "Date");
        }

        [Test]
        public void AddProperty_SetsFilterNameAsTextForOtherTypes()
        {
            AssertFilterNameFor(model => model.StringField, "Text");
        }

        [Test]
        public void GridColumn_SetsExpressionValueAsCompiledExpression()
        {
            GridModel gridModel = new GridModel { Name = "TestName" };

            String actual = column.ExpressionValue(gridModel) as String;
            String expected = "TestName";

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void GridColumn_SetsProcessorTypeAsPreProcessor()
        {
            GridProcessorType actual = new GridColumn<GridModel, Object>(grid, model => model.Name).ProcessorType;
            GridProcessorType expected = GridProcessorType.Pre;

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void GridColumn_OnNonMemberExpressionIsNotSortable()
        {
            Boolean? actual = new GridColumn<GridModel, String>(grid, model => model.ToString()).IsSortable;
            Boolean expected = false;

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void GridColumn_OnMemberExpressionIsSortableIsNull()
        {
            GridColumn<GridModel, Int32> column = new GridColumn<GridModel, Int32>(grid, model => model.Sum);

            Assert.IsNull(column.IsSortable);
        }

        [Test]
        public void GridColumn_OnNonMemberExpressionIsNotFilterable()
        {
            Boolean? actual = new GridColumn<GridModel, String>(grid, model => model.ToString()).IsFilterable;
            Boolean expected = false;

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void GridColumn_OnMemberExpressionIsFilterableIsNull()
        {
            GridColumn<GridModel, Int32> column = new GridColumn<GridModel, Int32>(grid, model => model.Sum);

            Assert.IsNull(column.IsFilterable);
        }

        [Test]
        public void GridColumn_SetsNameFromExpression()
        {
            Expression<Func<GridModel, String>> expression = (model) => model.Name;

            String actual = new GridColumn<GridModel, String>(grid, expression).Name;
            String expected = ExpressionHelper.GetExpressionText(expression);

            Assert.AreEqual(expected, actual);
        }

        #endregion

        #region Method: Process(IQueryable<T> items)

        [Test]
        public void Process_IfFilterIsNullReturnsItems()
        {
            column.Filter = null;
            column.IsSortable = false;
            column.IsFilterable = true;
            column.SortOrder = GridSortOrder.Desc;

            IQueryable<GridModel> expected = new GridModel[2].AsQueryable();
            IQueryable<GridModel> actual = column.Process(expected);

            Assert.AreSame(expected, actual);
        }

        [Test]
        public void Process_IfNotFilterableReturnsItems()
        {
            column.IsSortable = false;
            column.IsFilterable = false;
            column.SortOrder = GridSortOrder.Desc;
            column.Filter = Substitute.For<IGridFilter<GridModel>>();

            IQueryable<GridModel> expected = new GridModel[2].AsQueryable();
            IQueryable<GridModel> actual = column.Process(expected);

            Assert.AreSame(expected, actual);
        }

        [Test]
        public void Process_ReturnsFilteredItems()
        {
            column.IsSortable = false;
            column.IsFilterable = true;
            column.SortOrder = GridSortOrder.Desc;
            column.Filter = Substitute.For<IGridFilter<GridModel>>();
            IQueryable<GridModel> items = new GridModel[2].AsQueryable();
            IQueryable<GridModel> filteredItems = new GridModel[2].AsQueryable();

            column.Filter.Process(items).Returns(filteredItems);

            IQueryable<GridModel> actual = column.Process(items);
            IQueryable<GridModel> expected = filteredItems;

            Assert.AreSame(expected, actual);
        }

        [Test]
        public void Process_IfSortOrderIsNullReturnsItems()
        {
            column.IsFilterable = false;
            column.IsSortable = true;
            column.SortOrder = null;

            IQueryable<GridModel> expected = new GridModel[2].AsQueryable();
            IQueryable<GridModel> actual = column.Process(expected);

            Assert.AreSame(expected, actual);
        }

        [Test]
        public void Process_IfNotSortableReturnsItems()
        {
            column.IsSortable = false;
            column.IsFilterable = false;
            column.SortOrder = GridSortOrder.Desc;

            IQueryable<GridModel> expected = new GridModel[2].AsQueryable();
            IQueryable<GridModel> actual = column.Process(expected);

            Assert.AreSame(expected, actual);
        }

        [Test]
        public void Process_ReturnsItemsSortedInAscendingOrder()
        {
            column.IsSortable = true;
            column.IsFilterable = false;
            column.SortOrder = GridSortOrder.Asc;
            GridModel[] models = { new GridModel { Name = "B" }, new GridModel { Name = "A" }};

            IEnumerable expected = models.OrderBy(model => model.Name);
            IEnumerable actual = column.Process(models.AsQueryable());

            CollectionAssert.AreEqual(expected, actual);
        }

        [Test]
        public void Process_ReturnsItemsSortedInDescendingOrder()
        {
            column.IsSortable = true;
            column.IsFilterable = false;
            column.SortOrder = GridSortOrder.Desc;
            GridModel[] models = { new GridModel { Name = "A" }, new GridModel { Name = "B" } };

            IEnumerable expected = models.OrderByDescending(model => model.Name);
            IEnumerable actual = column.Process(models.AsQueryable());

            CollectionAssert.AreEqual(expected, actual);
        }

        [Test]
        public void Process_ReturnsFilteredAndSortedItems()
        {
            column.IsSortable = true;
            column.IsFilterable = true;
            column.SortOrder = GridSortOrder.Desc;
            column.Filter = Substitute.For<IGridFilter<GridModel>>();
            IQueryable<GridModel> models = new [] { new GridModel { Name = "A" }, new GridModel { Name = "B" }, new GridModel { Name = "C" } }.AsQueryable();
            column.Filter.Process(models).Returns(models.Where(model => model.Name != "A").ToList().AsQueryable());

            IEnumerable expected = models.Where(model => model.Name != "A").OrderByDescending(model => model.Name);
            IEnumerable actual = column.Process(models);

            CollectionAssert.AreEqual(expected, actual);
        }

        #endregion

        #region Method: ValueFor(IGridRow row)

        [Test]
        public void ValueFor_UsesExpressionValueToGetValue()
        {
            column.ExpressionValue = (model) => "TestValue";

            String actual = column.ValueFor(new GridRow(null)).ToString();
            String expected = "TestValue";

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ValueFor_OnNotNullRenderValueUsesRenderValueToGetValue()
        {
            column.RenderValue = (model) => "RenderValue";
            column.ExpressionValue = (model) => "ExpressionValue";

            String actual = column.ValueFor(new GridRow(null)).ToString();
            String expected = "RenderValue";

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ValueFor_OnNullReferenceInExpressionValueReturnsEmpty()
        {
            column.ExpressionValue = (model) => (null as String).Length;

            String actual = column.ValueFor(new GridRow(null)).ToString();
            String expected = "";

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ValueFor_OnNullReferenceInRenderValueReturnsEmpty()
        {
            column.RenderValue = (model) => (null as String).Length.ToString();
            column.ExpressionValue = (model) => "ExpressionValue";

            String actual = column.ValueFor(new GridRow(null)).ToString();
            String expected = "";

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ValueFor_ExpressionValue_ThrowsNotNullReferenceException()
        {
            column.ExpressionValue = (model) => Int32.Parse("Zero");

            Assert.Throws<FormatException>(() => column.ValueFor(new GridRow(null)));
        }

        [Test]
        public void ValueFor_RenderValue_ThrowsNotNullReferenceException()
        {
            column.RenderValue = (model) => Int32.Parse("Zero").ToString();

            Assert.Throws<FormatException>(() => column.ValueFor(new GridRow(null)));
        }

        [Test]
        [TestCase(null, "For {0}", true, "")]
        [TestCase(null, "For {0}", false, "")]
        [TestCase("<name>", null, false, "<name>")]
        [TestCase("<name>", null, true, "&lt;name&gt;")]
        [TestCase("<name>", "For <{0}>", false, "For <<name>>")]
        [TestCase("<name>", "For <{0}>", true, "For &lt;&lt;name&gt;&gt;")]
        public void ValueFor_GetsRenderValue(String value, String format, Boolean isEncoded, String expected)
        {
            IGridRow row = new GridRow(new GridModel { Name = value });
            column.RenderValue = (model) => model.Name;
            column.ExpressionValue = null;
            column.IsEncoded = isEncoded;
            column.Format = format;

            String actual = column.ValueFor(row).ToString();

            Assert.AreEqual(expected, actual);
        }

        [Test]
        [TestCase(null, "For {0}", true, "")]
        [TestCase(null, "For {0}", false, "")]
        [TestCase("<name>", null, false, "<name>")]
        [TestCase("<name>", null, true, "&lt;name&gt;")]
        [TestCase("<name>", "For <{0}>", false, "For <<name>>")]
        [TestCase("<name>", "For <{0}>", true, "For &lt;&lt;name&gt;&gt;")]
        public void ValueFor_GetsExpressionValue(String value, String format, Boolean isEncoded, String expected)
        {
            IGridRow row = new GridRow(new GridModel { Name = value });
            column.IsEncoded = isEncoded;
            column.Format = format;

            String actual = column.ValueFor(row).ToString();

            Assert.AreEqual(expected, actual);
        }

        #endregion

        #region Test helpers

        private void AssertFilterNameFor<TValue>(Expression<Func<AllTypesModel, TValue>> property, String expected)
        {
            IGrid<AllTypesModel> grid = Substitute.For<IGrid<AllTypesModel>>();
            grid.Query = new NameValueCollection();

            String actual = new GridColumn<AllTypesModel, TValue>(grid, property).FilterName;

            Assert.AreEqual(expected, actual);
        }

        #endregion
    }
}
