import { useEffect } from "react"
import { useLoaderData } from "react-router-dom"
import Post from "../../Components/Post/Post"
import LikeCard from '../../Components/LikeCard/LikeCard'
import './Home.css'
import { CURRENT_USER, postController, userController } from "../../Constants"
import React from "react"

export async function HomeLoader({params})
{
    const current_user = JSON.parse(localStorage.getItem(CURRENT_USER))
    const response = await fetch(postController + `/GetFeed/${current_user.userId}`)
    if (response.ok)
    {
        let arr = await response.json()
        let type='feed'
        if (arr.length == 0)
        {
            const response = await fetch(userController + `/GetRecommendedFriends/${current_user.userId}`)
            if (response.ok)
            {
                arr = await response.json()
                type='recommended'
            }
        }
        return {type,array:arr} 
    }
    return []
}

export default function Home()
{
    const { type, array:data } = useLoaderData(); 
    const [dataState, setDataState] = React.useState([])
    useEffect(() => {
        if (data.length > 0) {
            setDataState(data.map(item => {
                if (type == 'feed')
                    return <Post props={item} />
                return <LikeCard props={item} />
            }))
        }
        else setDataState([])
    }
        , [data])
    return <div className="home-container">
        {dataState}
       
    </div>
}