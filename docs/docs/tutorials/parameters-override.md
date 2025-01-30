# Hiding, Showing, and Modifying Parameters in the UI
In this tutorial, you will learn how to dynamically control the visibility and modification of parameters in the user interface (UI) by overriding the `GetParameters()` method in a Tickblaze script. This is particularly useful when you want to conditionally hide, show, or modify parameters based on certain conditions, such as the state of other parameters.

```csharp
namespace Tickblaze.Scripts.Tutorials;

public partial class ParametersSample : Indicator
{
    [Parameter("Show")]
    public bool Show { get; set; } = true;

    [Parameter("Optional Parameter")]
    public int OptionalParameter { get; set; } = 5;

    [Parameter("Status")]
    public string Status { get; set; } = "Shown";

    protected override Parameters GetParameters(Parameters parameters)
    {
        if (Show)
        {
            parameters[nameof(Status)].Value = "Shown";
        }
        else
        {
            parameters.Remove(nameof(OptionalParameter));

            parameters[nameof(Status)].Value = "Hidden";
        }

        return parameters;
    }

    protected override void Calculate(int index)
    {
        // In this example, no calculations are performed.
    }
}
```

---

## Explanation of the Code

### 1. **Parameters**
- **`Show`**: A boolean parameter that acts as a toggle. When `true`, the `OptionalParameter` is shown in the UI. When `false`, the `OptionalParameter` is hidden.
- **`OptionalParameter`**: An integer parameter that is conditionally displayed based on the value of the `Show` parameter.
- **`Status`**: A string parameter that displays the current status of the `OptionalParameter` (either "Shown" or "Hidden").

### 2. **`GetParameters()` Method**
- This method is overridden to dynamically modify the parameters displayed in the UI.
- If `Show` is `true`:
  - The `Status` parameter is set to `"Shown"`.
  - The `OptionalParameter` remains visible in the UI.
- If `Show` is `false`:
  - The `OptionalParameter` is removed from the UI using `parameters.Remove(nameof(OptionalParameter))`.
  - The `Status` parameter is set to `"Hidden"`.

### 3. **`Calculate()` Method**
- This method is required for all indicators but is left empty in this example since the script focuses on parameter management rather than calculations.

---

## How It Works
1. When the script is loaded, the `Show` parameter is set to `true` by default, so the `OptionalParameter` is visible, and the `Status` parameter displays `"Shown"`.
2. If the user sets the `Show` parameter to `false` in the UI:
   - The `OptionalParameter` is hidden.
   - The `Status` parameter updates to `"Hidden"`.
3. The `GetParameters()` method ensures that the UI is dynamically updated based on the value of the `Show` parameter.

---

## Usage Example
1. Add the `ParametersSample` indicator to your chart.
2. Open the indicator's settings in the UI.
3. Toggle the `Show` parameter between `true` and `false`.
4. Observe that the `OptionalParameter` is shown or hidden accordingly, and the `Status` parameter updates to reflect the current state.

---

## Other Customization Ideas
- **Dynamic Ranges**: Modify the range of a parameter based on another parameter's value.
- **Conditional Name**: Change name of parameters based on complex conditions.
