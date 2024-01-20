import { CURRENT_USER, userController } from "../../Constants";
import React, { useEffect, useState } from "react";
import { useLoaderData } from "react-router-dom";
import LikeCard from "../../Components/LikeCard/LikeCard";
export async function SearchLoader({ params }) {
    const user = JSON.parse(localStorage.getItem(CURRENT_USER))
    const result = await fetch(userController + `/SearchForUsers/${params.usernamePattern}/${user.userId}`)
    if (result.ok) {
        return await result.json()
    }
    else {
        alert("Error")
        return []
    }
    
}
export default function Search()
{
    const data = useLoaderData()
    const [state, setState] = useState([])
    
    useEffect(() =>
    {
        setState(data.map(item =>
        {
            return <LikeCard props={item} key={item.userId} />
            }))
    }, data)
    

    return <div style={{
        width: "100vw", height: "100vh", display: 'flex', flexDirection: 'column'
    }}>
        {state}
    </div>
}