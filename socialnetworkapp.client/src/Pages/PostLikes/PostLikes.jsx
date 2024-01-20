import { useLoaderData } from "react-router-dom";
import { CURRENT_USER, postController } from "../../Constants";
import { useEffect, useState } from "react";
import LikeCard from "../../Components/LikeCard/LikeCard";
import "./PostLikes.css"
export async function PostLikesLoader({ params })
{
    const user = JSON.parse(localStorage.getItem(CURRENT_USER))
    const result = await fetch(postController + `/GetLikes/${params.postId}/${user.userId}`)
    
    if (result.ok)
    {
        const jsonRes = await result.json()
        return jsonRes
    }
    alert('Something went wrong.')
    return []
}




export default function PostLikes()
{
    const data = useLoaderData()
    const [likeStates,setState]=useState([])
    useEffect(() =>
    {
        setState(data.map(like =>
        {
            return <LikeCard props={{username:like.username,userId:like.userId,thumbnail:like.thumbnail}}/>
            }))
    }, data)
    return <div className="post_likes_container">
        {likeStates}
    </div>
}