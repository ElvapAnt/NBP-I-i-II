import './UploadImage.css'


export default function UploadImage({ props}) {
    const { inputId, labelText, setState } = props
    function onFileUpload(event)
    {
    const file = event.target.files[0]
    if (file == null) return
    const fileReader = new FileReader()
    fileReader.readAsDataURL(file)
    fileReader.onload=(ev) =>
    {
        alert("Image uploaded.")
        setState(oldValue => {
            return fileReader.result
        })
    }
    }
    return <>
        <label className='upload-label' htmlFor={inputId}>{labelText}</label> <input id={inputId} type='file' onChange={onFileUpload}
            accept=".jpg, .jpeg, .png" ></input>
    </>
}