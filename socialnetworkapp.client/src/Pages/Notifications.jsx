import { useEffect, useState } from "react"
import { useLoaderData } from "react-router-dom"
import LikeCard from "../Components/LikeCard/LikeCard"
import {  CURRENT_USER, notificationController } from "../Constants"

export async function NotificationsLoader({ params })
{
    const user = JSON.parse(localStorage.getItem(CURRENT_USER))
    const res = await fetch(notificationController + `/GetReceivedRequests/${user.userId}`)
    if (res.ok)
        return await res.json()
    return []
}
export default function Notifications()
{
    const data = useLoaderData()
    const [state, setState] = useState([])
    
    useEffect(() =>
    {
        setState(data.map(item =>
        {
            return <LikeCard key={item.notificationId}
                props={{
                    notification: item,
                    thumbnail:item.thumbnail?item.thumbnail:'',
                    userId: item.url,
                    username:item.from
                }
                } />
            }))
    }, [data])
    return <div style={{
        width: '90%',
        display: 'flex',
        flexDirection: 'column',
        height:'100vh'
    }}>
        {state}
    </div>
}