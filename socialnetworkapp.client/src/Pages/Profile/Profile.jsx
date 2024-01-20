import { useLoaderData } from "react-router-dom";
import { CURRENT_USER, postController, userController } from "../../Constants";
import './Profile.css'
import { Button, TextField } from "@mui/material";
import { useState,useEffect } from "react";
import UploadImage from "../../Components/UploadImage/UploadImage";
import Post from "../../Components/Post/Post";
import React from "react";


export async function ProfileLoader({ params })
{
    const userId = params.userId
    const user = JSON.parse(localStorage.getItem(CURRENT_USER))
    const currentId = user.userId
    let result = fetch(userController + '/GetUser/' + userId+'/'+user.sessionToken)
    let response = fetch(postController + `/GetPosts/${userId}/${currentId}`)
    result = await result
    if (result.ok)
    {
        const returnValue = {}
        returnValue['profile'] = await result.json()
        response=await response
        if (response.ok)
        {
            returnValue['posts']=await response.json()
        }
        else
            returnValue['posts'] = []
        return returnValue
    }
    return null
}
export default function Profile()
{
    
    const {profile,posts}=useLoaderData()
    const { username, thumbnail, name, email, bio } = profile
    const [userState, setUserState] = useState({
        username,email,bio,thumbnail,name
    })
    const [newUsername, setNewUsername] = useState('')
    const [newFile,setNewFile] = useState(null)
    const [postComponents, setPosts] = React.useState([])
    useEffect(() =>
    {
        setPosts(posts.map(post =>
        {
            return <Post props={post} />
            }))
        }
        , [posts])
    async function handleClick()
    {
        if (newUsername != null && newUsername != '')
        {
            const res = await fetch(userController + `/UpdateUsername/${currentUser.userId}/${newUsername}`, {
                method:"PUT",
                headers: {
                  
                }
            })
            if (res.ok)
            {
                const newUser = { ...currentUser, username: newUsername }
                currentUser=newUser
                localStorage.setItem(CURRENT_USER, JSON.stringify(newUser))
                setUserState(oldValue => {
                    return {
                        ...oldValue,
                        username:newUsername
                    }
                })
            }
        }
        if (newFile != null)
        {
            const res = await fetch(userController + `/UpdateProfilePicture/${currentUser.userId}`,
                {
                    method: "PUT",
                    headers: {
                        'Content-Type': 'application/json',
                        'Accept':'*/*'
                    },
                    body:JSON.stringify(newFile)
                })
            if (res.ok)
            {
                const newUser = { ...currentUser, thumbnail:newFile}
                currentUser=newUser
                localStorage.setItem(CURRENT_USER, JSON.stringify(newUser))
                setUserState(oldValue => {
                    return {
                        ...oldValue,
                        thumbnail:newFile
                    }
                })
                }
            }
        
    }
    let currentUser = JSON.parse(localStorage.getItem(CURRENT_USER))
    const enabled = currentUser.username===username
    return <div className='profile-and-posts-container'>
            <div className="profile-container">
                <img src={userState.thumbnail} className="profile-picture"></img>
                <div className='profile-info-container'>
                    <h3>{userState.username}</h3>
                    <p>{userState.name}</p>
                    <p>{userState.email}</p>
                    <p>{userState.bio}</p>
                </div>
                {enabled && <div className='profile-change-info-container profile-info-container'>
                    <div className="change-username">
                        <TextField value={newUsername}
                            type="text"
                            label='New Username'
                            onChange={ev => (setNewUsername(ev.target.value))}
                        />
                        <Button sx={
                            {
                                height: '60px', marginLeft: '10px'
                            }
                        } onClick={ev => { handleClick() }}>Accept</Button>
                    </div>
                    <UploadImage props={{inputId:'change-picture',labelText:'Change profile picture.',setState:setNewFile}} />
                    
                    

                </div>}
            
        </div>
        <div className="posts-container">
            {postComponents}
        </div>
        </div>
}