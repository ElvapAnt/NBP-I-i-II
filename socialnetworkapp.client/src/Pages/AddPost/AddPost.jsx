import { TextField, Button, Alert } from "@mui/material"
import UploadImage from "../../Components/UploadImage/UploadImage"
import './AddPost.css'
import { useState } from "react"
import { CURRENT_USER, chatController, postController } from "../../Constants"
import { useNavigate } from "react-router-dom"


export default function AddPost()
{
    const currentUser = JSON.parse(localStorage.getItem(CURRENT_USER))
    const [formData, setFormData] = useState({ content: "", mediaURL: "" })
    const navigate = useNavigate()

    function setMediaURL(fun)
    {
        setFormData(oldValue =>
        {
            return {...oldValue,mediaURL:fun()}
            })
    }
    const [failedToPost,setFailedToPost]=useState({value:false,message:''})
    function onChange(ev)
    {
        setFormData(oldValue => {
            return {...oldValue,[ev.target.name]:ev.target.value}
        })
    }

    async function post()
    {
        if (formData.content == '' || formData.mediaURL == '')
            return
        const result = await fetch(postController + `/AddPost/${currentUser.userId}`, {
            method: "POST",
            headers: {
                'Content-Type': 'application/json',
                'Accept':'*/*',
            },
            body:JSON.stringify(formData)
        })
        if (result.ok)
        {
            alert('Posted successfully.')
            navigate('/home')
        }
        else
        {
            const msg = await result.text()
            setFailedToPost(oldValue =>
            {
                setTimeout(setFailedToPost({
                    value: true,
                    message:msg
                }))
                return oldValue
                })
            }
    }
    return <div className="add_post_container">
        <div className="login_container">
            <TextField id="Content" label="Content" variant="outlined" sx={{
                marginTop:"10px",
                marginBottom: "10px"
            }}
                value={formData.content}
                name='content'
            onChange={(onChange)}/>
        <img className="post_media" src = {formData.mediaURL}></img>
            <UploadImage props={{inputId:'upload-image',labelText:'Upload photo.',setState:setMediaURL} } />
            {failedToPost.value&& <Alert severity="error">Failed to post, reason: {failedToPost.message}</Alert>}
            <Button variant="text" sx={{
                width:"80px"
            }} id='Button' onClick={(ev) =>
            {
            post()
            }}>OK</Button>
        </div>

    </div>
}