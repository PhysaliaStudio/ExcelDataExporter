# ExcelDataExporter

## First Time Setup

1. Open the window by `Tools/ExcelDataExporter`
2. Select the folder where the `.xlsx` files are located.
3. Select the folder where the generated `.cs` files are located.
4. Select the folder where the exported data(`.asset`, `.json`) files are located.

## Export

1. Select the `.xlsx` files you want to export.
2. Select the file format.
3. Click the **Generate Code** button. This will generate the `.cs` files match the sheets.
4. Click the **Export** button.

## Sheet Format

The 1st row is only for `metadata` at (0, 0), see **Metadata** section for more information. Other than the (0, 0) cell, the whole 1st row is omitted, so you can the think as the 1st row of the sheet doesn't exist.

While thinking **the 1st row doesn't exist**:
- The 1st line is for `field summaries`. They will become the summaries of the generated code.
- The 2nd line is for `field names`. The names should follow the general naming convention.
- The 3rd line is for `field types`. Supported types are `int`, `string`, `bool`, `Vector2Int`, `Vector3Int`, custom types defined in the `$CustomTypes.xlsx`, and any array of these types.
- Why do we use the word 'line' here? Since the layout can be either `horizontal` or `vertical`, the 'line' can means 'row' or 'column'.

### Sheet Example

| name=ItemData ||           |           | <- Metadata Line
| :---: | :---:  | :---:     | :---:     | --- |
| ItemId | Name  | Minimum Damage | Maximum Damage | <- Field Summary Line |
| id    | name   | damageMin | damageMax | <- Field Name Line |
| int   | string | int       | int       | <- Field Type Line |
| 1001  | Sword  | 5         | 8         | <- Data Start Line |
| 1002  | Axe    | 3         | 12        |                    |

Click the **Generate Code** button. The exporter generates the following `.cs` files.

The following code is generated in Asset mode. In Json mode it will be a little different.

```csharp
public class ItemDataTable : DataTable<ItemData>
{

}
```

```csharp
[Serializable]
public class ItemData
{
    [SerializeField]
    private int _id;
    [SerializeField]
    private string _name;
    [SerializeField]
    private int _damageMin;
    [SerializeField]
    private int _damageMax;

    /// <summary>
    /// ItemId
    /// </summary>
    public int Id => _id;

    /// <summary>
    /// Name
    /// </summary>
    public string Name => _name;

    /// <summary>
    /// Minimum Damage
    /// </summary>
    public int DamageMin => _damageMin;

    /// <summary>
    /// Maximum Damage
    /// </summary>
    public int DamageMax => _damageMax;
}
```

## Metadata

- Metadata tells the exporter how to treat the sheet.
- Metadata should be in the first cell (0, 0) of each sheet.
- The format is `key=value`.

The supported keys are:
- `name`: The name of the sheet. The default class name would be the name of the sheet. I mean the **Sheet**! Not the .xlsx file name, that's **Worksheet**!
- `namespace`: The namespace of the generated code. The default namespace would be *no namespace*.
- `type`: The type of the sheet. The default type is `data-table`. The other option is `setting`. The only difference between them is that, `DataTable` needs an `int id` field, since `Dictionary` needs a key, while `Setting` doesn't need `Dictionary`.
- `layout`: The layout of the sheet. The default layout is `horizontal`. The other option is `vertical`. In default, `DataTable` is in horizontal layout, and `Setting` is in vertical layout. So normally you don't need to set this.
- `export`: Whether to export the sheet. The default value is `true`. The other option is `false`.

For example:
```
name=ItemDrop  # The generated class name will be ItemDrop.
namespace=Game.Data  # The generated code will be in the Game.Data namespace.
type=setting  # This sheet is a setting sheet.
layout=vertical  # This sheet is in vertical layout.
export=false  # This sheet will not be exported. (Maybe designers are still working on it.)
```

## Ignored Files

- Exporter only considers .xlsx files, other type of files will be ignored.
- The files start with `~` will be ignored. Normally they are temp files. For example, `~$ItemDrop.xlsx` will be ignored.
- The files start with `$` will be ignored, and some of them are for special purposes. For example, `$CustomTypes.xlsx` will be treatd as custom type definitions.

## Custom Types

- You may have some small custom types that you want to use in your data. For example, you may have a `Reward` type that you want to use in your data.
- `$CustomTypes.xlsx` is a special file. You can define your custom types in this file.
- The class or struct are defined in the following format:
  |                              |              |              |       |
  | :---:                        | :---:        | :---:        | :---: |
  | `<class\|struct> <typename>` | Summary 1    | Summary 2    | ...   |
  |                              | Field Name 1 | Field Name 2 | ...   |
  |                              | Field Type 1 | Field Type 2 | ...   |

- The enum is defined in the following format:
  |                   |           |           |       |
  | :---:             | :---:     | :---:     | :---: |
  | `enum <typename>` | Summary 1 | Summary 2 | ...   |
  |                   | Name 1    | Name 2    | ...   |
  |                   | Value 1   | Value 2   | ...   |

- The metadata works a little different for custom types. Only `namespace` is supported. The default namespace is *no namespace*.
- This file supports multiple custom types. The exporter will generate the `.cs` files for each type.
- This file supports multiple sheets. It's useful when you need different namespaces for different types.

### Custom Type Example

The following example is in the `$CustomTypes.xlsx` file.
|               |           |         |         |            |
| :---:         | :---:     | :---:   | :---:   | :---:      |
| struct Reward |           |         |         |            |
|               | itemId    | count   |         |            |
|               | int       | int     |         |            |
|               |           |         |         |            |
| enum ItemType |           |         |         |            |
|               | Undefined | Weapon  | Armor   | Consumable |
|               | 0         | 1       | 2       | 10         |

Click the **Generate Code** button. The exporter generates the `.cs` files. Each type will be generated in a separate file.
```csharp
public struct Reward
{
    public int itemId;
    public int count;
}
```
```csharp
public enum ItemType
{
    Undefined = 0,
    Weapon = 1,
    Armor = 2,
    Consumable = 10,
}
```

## Limitations

### Compile Errors

This tool is just an tool, generated code may cause compile error if you don't use it properly. Here are some tips to avoid compiled errors.
- Do it safely! No compile error for each step!
- If you generate some code and get compile errors, you'd better go back to the last clean state by VCS.
- If you want to change field names or delete some fields, it's better that programmers and designers discuss together.

The safest way is the *standard* refactor process:
1. Designers add some new fields, keep deprecated fields, and export. No compile error? Commit!
2. Programmers remove the old code, and work with new fields. No compile error? Commit!
3. Designers should be safe to remove deprecated fields now, and export again. No compile error? Commit!

Follow these steps, there should be no compiled errors after each step, therefore it should be safe to commit for each step.

### Out of Unity

For now, this tool is designed to work in Unity, due to generating ScriptableObject with reflection. Obviously this tool should support out of Unity, but it's not implemented yet. The ideal approach is to have a common dll, a cross-platform GUI application, and a Unity editor for ScriptableObject. At that time, there will be a new project and this project will be deprecated.
