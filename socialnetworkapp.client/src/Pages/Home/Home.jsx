import { useEffect } from "react"
import { useLoaderData } from "react-router-dom"
import Post from "../../Components/Post/Post"
import './Home.css'
import { CURRENT_USER, postController } from "../../Constants"
import React from "react"

export async function HomeLoader({params})
{
    const current_user = JSON.parse(localStorage.getItem(CURRENT_USER))
    const response = await fetch(postController + `/GetFeed/${current_user.userId}`)
    if (response.ok)
    {
        return await response.json()
    }
    return []
}

export default function Home()
{
    const posts = useLoaderData() 
    const [postComponents, setPosts] = React.useState([])
    useEffect(() =>
    {
        setPosts(posts.map(post =>
        {
            return <Post props={post} />
            }))
        }
        , [posts])
    return <div className="home-container">
        {postComponents}
    </div>
}