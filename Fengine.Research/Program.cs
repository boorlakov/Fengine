var slaeSolver = new Fengine.Backend.LinearAlgebra.SlaeSolver.LocalOptimalScheme();
var integrator = new Fengine.Backend.Integration.GaussFourPoints();
var matrixType = new Fengine.Backend.LinearAlgebra.Matrix.Sparse();

// Arrange
var inputFuncs = new[]
{
    # region By time

    // Const
    new Fengine.Backend.DataModels.InputFuncs
    {
        Lambda = "1",
        UStar = "5",
        RhsFunc = "0",
        Sigma = "1",
        Chi = "1"
    },

    // Linear
    new Fengine.Backend.DataModels.InputFuncs
    {
        Lambda = "1",
        UStar = "5",
        RhsFunc = "0",
        Sigma = "1",
        Chi = "1"
    },

    // Quadratic
    new Fengine.Backend.DataModels.InputFuncs
    {
        Lambda = "1",
        UStar = "5",
        RhsFunc = "0",
        Sigma = "1",
        Chi = "1"
    },

    // Cubic
    new Fengine.Backend.DataModels.InputFuncs
    {
        Lambda = "1",
        UStar = "5",
        RhsFunc = "0",
        Sigma = "1",
        Chi = "1"
    },

    // In fourth
    new Fengine.Backend.DataModels.InputFuncs
    {
        Lambda = "1",
        UStar = "5",
        RhsFunc = "0",
        Sigma = "1",
        Chi = "1"
    },

    // In fifth
    new Fengine.Backend.DataModels.InputFuncs
    {
        Lambda = "1",
        UStar = "5",
        RhsFunc = "0",
        Sigma = "1",
        Chi = "1"
    },

    // In cosine
    new Fengine.Backend.DataModels.InputFuncs
    {
        Lambda = "1",
        UStar = "5",
        RhsFunc = "0",
        Sigma = "1",
        Chi = "1"
    },

    // Exponential
    new Fengine.Backend.DataModels.InputFuncs
    {
        Lambda = "1",
        UStar = "5",
        RhsFunc = "0",
        Sigma = "1",
        Chi = "1"
    },

    #endregion
};

var boundaryConds = new[]
{
    # region By time

    // Const
    new Fengine.Backend.DataModels.Conditions.Boundary.TwoDim
    {
        LeftType = "First",
        LeftFunc = "5",
        RightType = "First",
        RightFunc = "5",
        LowerType = "First",
        LowerFunc = "5",
        UpperType = "First",
        UpperFunc = "5"
    },

    #endregion
};