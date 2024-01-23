import { useLoaderData } from "react-router-dom";
import { CURRENT_USER, postController } from "../../Constants";
import { useEffect, useState } from "react";
import Post from "../../Components/Post/Post";
import SearchBar from "../../Components/Searchbar/Searchbar";
import SendSharp from "@mui/icons-material/SendSharp";


export async function PostCommentsLoader({ params })
{
    const currentUser = JSON.parse(localStorage.getItem(CURRENT_USER))
    const result = await fetch(postController + '/GetComments/' + params.postId+'/'+currentUser.userId)
    if (result.ok)
    {
        return { array: await result.json(), postId:params.postId }
    }
    alert('Something went wrong')
    return []
}




export default function PostComments()
{
    const data = useLoaderData()
    const [dataState, setState] = useState([])
    useEffect(() => {
        setState(

            data.array
        )
    }, [JSON.stringify(data.array)])
    
    const [comment,setComment]=useState('')
    async function postComment()
    {
        const user = JSON.parse(localStorage.getItem(CURRENT_USER))
        const result = await fetch(postController + `/AddComment/${user.userId}/${data.postId}`,
            {
                method: 'POST', headers: {
            'Content-Type':'application/json'
                }, body: JSON.stringify({ content: comment })
            })
        if (result.ok)
        {
            location.reload()
            return
        }
        alert('Something went wrong.')
        return
    }
    return <div style={{
        "width":"100%","height":"100%","display":"flex","flexDirection":"column"
    }}>
        {dataState.map(item =>
        {
            return <Post props={item} key={item.postId} parentPostId={data.postId} />
            })}
        <div style={{
            position: 'absolute',
            bottom: '0px',
            display: 'flex',
            flexDirection: 'column',
            alignItems: 'center',
            backgroundColor: 'var(--sky)',
            padding: '10px',
            width:'100%'
        }}>
            <SearchBar query={comment} setQuery={setComment} onQueryExecute={postComment}
                customization={{ buttonImg:<SendSharp/>, placeholder: 'Leave a comment' }}/>
        </div>

    </div>
}