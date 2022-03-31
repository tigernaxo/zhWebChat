let file = {
  isImage: function(fileName) {
    if (!fileName) return false
    let arr = fileName.toUpperCase().split('.')
    return arr.length < 2
      ? false
      : ['JPG', 'JPEG', 'BMP', 'GIF', 'PNG'].includes(arr[arr.length - 1])
  }
}
export default file
