namespace Tomat.Hacksaw.Metadata.Image;

/* if global variable type is obj or struct:
 *     global = globals[globalIndex]
 *     for (var i = 0; i < fields.length; i++) {
 *         var index = fields[i];
 *         var type = global.obj.fields[i].type;
 *         i32: val = ints[index]
 *         bool: val = index != 0
 *         f64: val = index != 0 // code.floats[index]
 *         bytes: val = index != 0 // code.getString(index);
 *         type: val = code.types[index]
 *         default: globals_data + globals_indexes[index] // what
 *
 *     }
 *  else: exception
 */
public readonly record struct ImageConstant(int GlobalIndex, int[] Fields);