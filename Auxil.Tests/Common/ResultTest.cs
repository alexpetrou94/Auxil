using JetBrains.Annotations;
using Auxil.Common;

namespace Auxil.Tests.Common;

[TestSubject(typeof(Result<>))]
public class ResultTest
{
    [Fact]
    public void Ok_IsSuccessWithValue()
    {
        Result<int> result = Result<int>.Ok(42);

        result.IsSuccess.ShouldBeTrue();
        result.IsFailure.ShouldBeFalse();
        result.Value.ShouldBe(42);
        result.ErrorMessage.ShouldBe(string.Empty);
    }

    [Fact]
    public void Error_IsFailureWithMessage()
    {
        Result<int> result = Result<int>.Error("boom");

        result.IsSuccess.ShouldBeFalse();
        result.IsFailure.ShouldBeTrue();
        result.Value.ShouldBe(0);
        result.ErrorMessage.ShouldBe("boom");
    }

    [Fact]
    public void GetValueOrDefault_ReturnsValueOnSuccess()
    {
        Result<int> result = Result<int>.Ok(42);

        result.GetValueOrDefault(0).ShouldBe(42);
    }

    [Fact]
    public void GetValueOrDefault_ReturnsDefaultOnFailure()
    {
        Result<int> result = Result<int>.Error("boom");

        result.GetValueOrDefault(0).ShouldBe(0);
    }

    [Fact]
    public void Match_Func_OnSuccess()
    {
        Result<int> result = Result<int>.Ok(42);

        string output = result.Match(value => $"ok:{value}", error => $"error:{error}");

        output.ShouldBe("ok:42");
    }

    [Fact]
    public void Match_Func_OnError()
    {
        Result<int> result = Result<int>.Error("boom");

        string output = result.Match(value => $"ok:{value}", error => $"error:{error}");

        output.ShouldBe("error:boom");
    }

    [Fact]
    public void Match_Action_OnSuccess()
    {
        Result<int> result = Result<int>.Ok(42);
        string? captured = null;

        result.Match(value => { captured = $"ok:{value}"; }, error => { captured = $"error:{error}"; });

        captured.ShouldBe("ok:42");
    }

    [Fact]
    public void Match_Action_OnError()
    {
        Result<int> result = Result<int>.Error("boom");
        string? captured = null;

        result.Match(value => { captured = $"ok:{value}"; }, error => { captured = $"error:{error}"; });

        captured.ShouldBe("error:boom");
    }

    [Fact]
    public void Map_TransformsValueOnSuccess()
    {
        Result<int> result = Result<int>.Ok(21);

        Result<string> mapped = result.Map(value => (value * 2).ToString());

        mapped.IsSuccess.ShouldBeTrue();
        mapped.Value.ShouldBe("42");
    }

    [Fact]
    public void Map_PreservesErrorOnFailure()
    {
        Result<int> result = Result<int>.Error("boom");

        Result<string> mapped = result.Map(value => value.ToString());

        mapped.IsFailure.ShouldBeTrue();
        mapped.ErrorMessage.ShouldBe("boom");
    }

    [Fact]
    public void Bind_ChainsOnSuccess()
    {
        Result<int> result = Result<int>.Ok(42);

        Result<string> bound = result.Bind(value => Result<string>.Ok(value.ToString()));

        bound.IsSuccess.ShouldBeTrue();
        bound.Value.ShouldBe("42");
    }

    [Fact]
    public void Bind_ShortCircuitsOnFailure()
    {
        Result<int> result = Result<int>.Error("boom");
        bool binderCalled = false;

        Result<string> bound = result.Bind(value =>
        {
            binderCalled = true;
            return Result<string>.Ok(value.ToString());
        });

        bound.IsFailure.ShouldBeTrue();
        bound.ErrorMessage.ShouldBe("boom");
        binderCalled.ShouldBeFalse();
    }

    [Fact]
    public void Deconstruct_YieldsStateAndValue()
    {
        Result<int> success = Result<int>.Ok(42);
        Result<string> failure = Result<string>.Error("boom");

        (bool successOk, int successValue) = success;
        (bool failureOk, string? failureValue) = failure;

        successOk.ShouldBeTrue();
        successValue.ShouldBe(42);
        failureOk.ShouldBeFalse();
        failureValue.ShouldBeNull();
    }

    [Fact]
    public void Catch_ReturnsOkWhenActionSucceeds()
    {
        Result<int> result = Result<int>.Catch(() => 42);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(42);
    }

    [Fact]
    public void Catch_ReturnsErrorWhenActionThrows()
    {
        Result<int> result = Result<int>.Catch(() => throw new InvalidOperationException("kaput"));

        result.IsFailure.ShouldBeTrue();
        result.ErrorMessage.ShouldBe("kaput");
    }

    [Fact]
    public void Error_ThrowsOnNullMessage()
    {
        Should.Throw<ArgumentNullException>(() => Result<int>.Error(null!));
    }

    [Fact]
    public void NonGeneric_Ok_IsSuccess()
    {
        Result result = Result.Ok();

        result.IsSuccess.ShouldBeTrue();
        result.ErrorMessage.ShouldBe(string.Empty);
    }

    [Fact]
    public void NonGeneric_Error_IsFailure()
    {
        Result result = Result.Error("boom");

        result.IsFailure.ShouldBeTrue();
        result.ErrorMessage.ShouldBe("boom");
    }

    [Fact]
    public void NonGeneric_Match_Func()
    {
        Result ok = Result.Ok();
        Result error = Result.Error("boom");

        ok.Match(() => "ok", message => $"error:{message}").ShouldBe("ok");
        error.Match(() => "ok", message => $"error:{message}").ShouldBe("error:boom");
    }

    [Fact]
    public void NonGeneric_Match_Action()
    {
        Result ok = Result.Ok();
        Result error = Result.Error("boom");
        string? captured = null;

        ok.Match(() => { captured = "ok"; }, message => { captured = message; });
        captured.ShouldBe("ok");

        error.Match(() => { captured = "ok"; }, message => { captured = message; });
        captured.ShouldBe("boom");
    }

    [Fact]
    public void NonGeneric_Catch()
    {
        Result ok = Result.Catch(() => { });
        Result error = Result.Catch(() => throw new InvalidOperationException("kaput"));

        ok.IsSuccess.ShouldBeTrue();
        error.IsFailure.ShouldBeTrue();
        error.ErrorMessage.ShouldBe("kaput");
    }

    [Fact]
    public void NonGeneric_Deconstruct_YieldsStateAndMessage()
    {
        Result ok = Result.Ok();
        Result error = Result.Error("boom");

        (bool okSuccess, string okMessage) = ok;
        (bool errorSuccess, string errorMessage) = error;

        okSuccess.ShouldBeTrue();
        okMessage.ShouldBe(string.Empty);
        errorSuccess.ShouldBeFalse();
        errorMessage.ShouldBe("boom");
    }

    [Fact]
    public void NonGeneric_Error_ThrowsOnNullMessage()
    {
        Should.Throw<ArgumentNullException>(() => Result.Error(null!));
    }
}
