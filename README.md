## `EB_AFIT_improved`

### Доработанный алгоритм, в нем решена пролема упаковки слоя в глубину, когда оставшееся свободное пространство не используется.

Предметы:
| Длина | Ширина | Высота | Количество |
| ----- | ------ | ------ | ---------- |
| 4     | 4      | 5      | 3          |
| 1     | 1      | 1      | 48         |

Контейнер:
| Длина | Ширина | Высота |
| ----- | ------ | ------ |
| 4     | 9      | 8      |

| EB_AFIT | EB_AFIT_improved |
| ----- | ------ |
| <img src="./images/problem-1.png" width="300"> | <img src="./images/problem-2.png" width="300"> |

### Увеличена производительность.

<img src="./images/performance-1.png" >
<img src="./images/performance-2.png" >

## `XYZRotationVertical`

### Вертикальная упаковка с вращением предметов в любой плоскости.

Может быть настроена таким образом, что бы не смешивать разные предметы один поверх другого.
```csharp
protected override bool SkipBoxBehind(int j) => itemsToPack[j].ID != itemsToPack[cboxi].ID;
```

<img src="./images/xyz-rotation-vertical.png" width="300">

## `WithoutRotation`

### Упаковка без вращения предметов.

<img src="./images/without-rotation.png" width="300">

## `ZRotation`

### Упаковка с вращением предметов в горизонтальной плоскости.

<img src="./images/z-rotation.png" width="300">
